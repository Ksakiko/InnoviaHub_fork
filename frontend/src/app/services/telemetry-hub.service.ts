import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HubConnection } from '@microsoft/signalr';
import { AppConfigService } from '../core/app-config.service';

@Injectable({
  providedIn: 'root',
})
export class TelemetryHubService {
  private connection?: HubConnection;

  constructor(private cfg: AppConfigService) {}

  async start(): Promise<void> {
    if (this.connection) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.cfg.telemetryHubUrl, {
        transport: signalR.HttpTransportType.WebSockets,
        skipNegotiation: true,
      })
      .withAutomaticReconnect()
      .build();
    try {
      await this.connection.start();
      console.log('SignalR connected.');
    } catch (err) {
      console.log('Error establishing SignalR connection: ' + err);
    }
  }

  useConnection(): signalR.HubConnection | undefined {
    return this.connection;
  }

  async registerTenantSlug(tenantSlug: string): Promise<any> {
    await this.connection
      ?.invoke('JoinTenant', tenantSlug)
      .catch((err) => console.error(err));
  }
}
