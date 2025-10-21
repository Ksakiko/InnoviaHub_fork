import { Component, inject, OnInit, signal } from '@angular/core';
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
export class SensorPaneComponent implements OnInit {
  tenantSlug = 'innovia';
  devices = signal<Device[]>([]);
  devicesWithRealtimeData = signal<DeviceWithRealtimeData[]>([]);
  // alerts = signal<Alert[]>([]);
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

      this.telemetryHubService
        .useConnection()
        ?.on('alertRaised', (data: RealtimeAlert) => {
          // console.log('Alert: ', data);

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

          // const existingAlert = this.alerts().find(
          //   (a) => a.ruleId === data.ruleId
          // );

          // this.alerts.update((prev) => [...prev, data]);
        });
    });
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

  runToast() {
    this.toastr.warning(
      `${this.alert()?.message}`,
      `${this.alert()?.severity.toUpperCase()} - ${
        this.alert()?.deviceName
      } (${this.alert()?.type.toUpperCase()})`,
      {
        timeOut: 7000,
      }
    );
  }
}
