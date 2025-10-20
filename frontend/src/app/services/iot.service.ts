import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AppConfigService } from '../core/app-config.service';
import { catchError, Observable, throwError } from 'rxjs';
import { Devices } from '../../types/iot-types';

@Injectable({
  providedIn: 'root',
})
export class IotService {
  private readonly url: string;
  private readonly tenantId: string = 'd98acc3d-e6bd-46f4-ba46-cc49300f26c8';

  constructor(private http: HttpClient, private cfg: AppConfigService) {
    this.url = `${this.cfg.iotApiUrl}/api/tenants/${this.tenantId}/devices`;
  }

  getAllDevices(): Observable<Devices[]> {
    const httpOptions = {
      Headers: new HttpHeaders({
        'Content-Type': 'application/json',
      }),
      observe: 'response' as const,
    };

    return this.http.get<Devices[]>(this.url).pipe(
      catchError((error) => {
        return throwError(() => error);
      })
    );
  }
}
