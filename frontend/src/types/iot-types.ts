export type Device = {
  id: string;
  tenantId: string;
  roomId: string;
  model: string;
  serial: string;
  status: string;
};

export type RealtimeData = {
  deviceId: string;
  serial: string;
  time: string;
  type: string;
  value: number;
  unit: string;
};

export type DeviceWithRealtimeData = {
  id: string;
  roomId: string;
  model: string;
  serial: string;
  status: string;
  temperature?: number;
  temperatureUnit?: string;
  temperatureTime?: string;
  co2?: number;
  co2Unit?: string;
  co2Time?: string;
};

export type RealtimeAlert = {
  tenantId: string;
  tenantSlug: string;
  deviceId: string;
  type: string;
  value: number;
  time: string;
  ruleId: string;
  severity: string;
  message: string;
};

export type Alert = {
  deviceName: string;
  type: string;
  time: string;
  severity: string;
  message: string;
};
