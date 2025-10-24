import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { IotService } from '../../services/iot.service';
import { AuthService } from '../../services/auth.service';
import {
  Alert,
  Device,
  DeviceWithRealtimeData,
  RealtimeAlert,
  RealtimeData,
} from '../../../types/iot-types';
import { TelemetryHubService } from '../../services/telemetry-hub.service';

@Component({
  selector: 'app-sensor-pane',
  imports: [],
  templateUrl: './sensor-pane.component.html',
  styleUrl: './sensor-pane.component.css',
})
export class SensorPaneComponent implements OnInit, OnDestroy {
  tenantSlug = 'innovia';
  devices = signal<Device[]>([]);
  devicesWithRealtimeData = signal<DeviceWithRealtimeData[]>([]);
  alert = signal<Alert | null>(null);

  private iotService = inject(IotService);
  private authService = inject(AuthService);
  private telemetryHubService = inject(TelemetryHubService);
  private toastr = inject(ToastrService);

  async ngOnInit(): Promise<void> {
    if (!this.authService.isAdmin()) return console.error('Unauthorized');

    // Fetch all registered devices in database
    this.getAllDevices();

    // Start SignalR
    this.telemetryHubService.start().then(async () => {
      // Register tenalnt slug
      await this.telemetryHubService.registerTenantSlug(this.tenantSlug);

      // Get realtime data and store it in devicesWithRealtimeData
      this.telemetryHubService
        .useConnection()
        ?.on('measurementReceived', (data: RealtimeData) => {
          this.handleRealtimeData(data);
        });

      // Get realtime alert data, store it, and print it out as alert toast in UI
      this.telemetryHubService
        .useConnection()
        ?.on('alertRaised', (data: RealtimeAlert) => {
          this.handleAlertData(data);
        });
    });
  }

  ngOnDestroy(): void {
    this.telemetryHubService.stop();
  }

  getAllDevices() {
    if (!this.authService.isAdmin()) return console.error('Unauthorized');

    this.iotService.getAllDevices().subscribe({
      next: (data) => {
        data.forEach((x) => {
          const newDevice: Device = {
            id: x.id,
            tenantId: x.tenantId,
            roomId: x.roomId,
            model: x.model,
            serial: x.serial,
            status: x.status,
          };

          if (this.devices().length < 1) {
            this.devices.set([newDevice]);
          } else {
            this.devices.update((prev) => [...prev, newDevice]);
          }
        });
      },
      error: (err) => {
        console.error(err);
      },
      complete: () => {},
    });
  }

  handleRealtimeData(data: RealtimeData) {
    const device = this.devices().find((d) => d.id === data.deviceId);

    if (!device) return;

    // Round up to two decimal places if data type is temperature
    const dataValue =
      data.type === 'temperature'
        ? Math.ceil(data.value * 100) / 100
        : data.value;

    const localDateTime = new Date(data.time).toLocaleString();

    const index = this.devicesWithRealtimeData().findIndex(
      (d) => d.id === device.id
    );

    // If devicesWithRealtimeData does not contain the device's realtime data, create and add a new object
    if (index < 0) {
      let newDevice: DeviceWithRealtimeData = {
        id: device.id,
        roomId: device.roomId,
        model: device.model,
        serial: device.serial,
        status: device.status,
        [data.type]: dataValue,
        [`${data.type}Unit`]: data.unit,
        [`${data.type}Time`]: localDateTime,
      };

      this.devicesWithRealtimeData.update((prev) => [...prev, newDevice]);
    } else {
      // Update the existing device data with new realtime data
      this.devicesWithRealtimeData.update((prev) => [
        ...prev.slice(0, index),
        {
          ...prev[index],
          [data.type]: dataValue,
          [`${data.type}Unit`]: data.unit,
          [`${data.type}Time`]: localDateTime,
        },
        ...prev.slice(index + 1),
      ]);
    }
  }

  handleAlertData(data: RealtimeAlert) {
    // Get the corresponding device info for the real-time data
    const device = this.devices().find((d) => d.id === data.deviceId);

    const newAlert: Alert = {
      deviceName: device ? device.serial : data.deviceId,
      type: data.type,
      time: data.time,
      severity: data.severity,
      message: data.message,
    };

    this.alert.set(newAlert);

    this.runToast();
  }

  runToast() {
    if (this.alert()?.severity === 'warning') {
      this.toastr.warning(
        // Alert message
        `${this.alert()?.message}`,
        // Alert toast title - e.g. "WARNING - dev-101 (CO2)"
        `${this.alert()?.severity.toUpperCase()} - ${
          this.alert()?.deviceName
        } (${this.alert()?.type.toUpperCase()})`,
        {
          // Display time
          timeOut: 7000,
        }
      );
    } else {
      this.toastr.info(
        // Alert message
        `${this.alert()?.message}`,
        // Alert toast title - e.g. "WARNING - dev-101 (CO2)"
        `Alert - ${
          this.alert()?.deviceName
        } (${this.alert()?.type.toUpperCase()})`,
        {
          // Display time
          timeOut: 7000,
        }
      );
    }
    // Other specific cases for different severities can be added later
  }
}
