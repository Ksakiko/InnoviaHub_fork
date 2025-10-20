import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { IotService } from '../../services/iot.service';
import { AuthService } from '../../services/auth.service';
import {
  Devices,
  DeviceWithRealtimeData,
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
  devices = signal<Devices[]>([]);
  devicesWithRealtimeData = signal<DeviceWithRealtimeData[]>([]);

  private iotService = inject(IotService);
  private authService = inject(AuthService);
  private telemetryHubService = inject(TelemetryHubService);
  index: any;

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
    });
  }

  getAllDevices() {
    if (!this.authService.isAdmin()) return console.error('Unauthorized');

    this.iotService.getAllDevices().subscribe({
      next: (data) => {
        data.forEach((x) => {
          const newDevice: Devices = {
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
}
