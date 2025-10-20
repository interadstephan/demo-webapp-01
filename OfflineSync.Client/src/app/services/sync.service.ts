import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DatabaseService, AppDatabase } from './database.service';
import { firstValueFrom } from 'rxjs';

export interface SyncRequest {
  agentId: string;
  deviceId: string;
  lastSyncVersion: number;
  pushedRecords: any[];
  pushedFiles: any[];
}

export interface SyncResponse {
  currentVersion: number;
  updatedRecords: any[];
  updatedFiles: any[];
  success: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class SyncService {
  private apiUrl = 'http://localhost:5000/api/sync';
  private agentId: string = '';
  private deviceId: string = '';
  private lastSyncVersion: number = 0;

  constructor(
    private http: HttpClient,
    private dbService: DatabaseService
  ) {
    // Generate or retrieve device ID
    this.deviceId = localStorage.getItem('deviceId') || this.generateDeviceId();
    localStorage.setItem('deviceId', this.deviceId);
  }

  setAgentId(agentId: string): void {
    this.agentId = agentId;
    const storedVersion = localStorage.getItem(`lastSyncVersion_${agentId}_${this.deviceId}`);
    this.lastSyncVersion = storedVersion ? parseInt(storedVersion, 10) : 0;
  }

  async performSync(): Promise<boolean> {
    if (!this.agentId) {
      console.error('Agent ID not set');
      return false;
    }

    const db = this.dbService.getDatabase();
    if (!db) {
      console.error('Database not initialized');
      return false;
    }

    try {
      // Get local changes to push to server
      const pushedRecords = await this.getLocalChanges(db);
      const pushedFiles = await this.getLocalFileChanges(db);

      const syncRequest: SyncRequest = {
        agentId: this.agentId,
        deviceId: this.deviceId,
        lastSyncVersion: this.lastSyncVersion,
        pushedRecords,
        pushedFiles
      };

      // Send sync request to server
      const response = await firstValueFrom(
        this.http.post<SyncResponse>(`${this.apiUrl}/sync`, syncRequest)
      );

      if (response.success) {
        // Apply server changes to local database
        await this.applyServerChanges(db, response);

        // Update last sync version
        this.lastSyncVersion = response.currentVersion;
        localStorage.setItem(
          `lastSyncVersion_${this.agentId}_${this.deviceId}`,
          this.lastSyncVersion.toString()
        );

        console.log('Sync completed successfully');
        return true;
      } else {
        console.error('Sync failed:', response.message);
        return false;
      }
    } catch (error) {
      console.error('Sync error:', error);
      return false;
    }
  }

  private async getLocalChanges(db: AppDatabase): Promise<any[]> {
    const records = await db.datarecords
      .find({
        selector: {
          version: {
            $gt: this.lastSyncVersion
          }
        }
      })
      .exec();

    return records.map(doc => doc.toJSON());
  }

  private async getLocalFileChanges(db: AppDatabase): Promise<any[]> {
    const files = await db.fileattachments
      .find({
        selector: {
          version: {
            $gt: this.lastSyncVersion
          }
        }
      })
      .exec();

    return files.map(doc => doc.toJSON());
  }

  private async applyServerChanges(db: AppDatabase, response: SyncResponse): Promise<void> {
    // Apply record updates
    for (const record of response.updatedRecords) {
      await db.datarecords.upsert({
        id: record.id,
        agentId: record.agentId,
        title: record.title,
        description: record.description,
        data: record.data,
        updatedAt: record.updatedAt,
        isDeleted: record.isDeleted,
        version: record.version
      });
    }

    // Apply file updates
    for (const file of response.updatedFiles) {
      await db.fileattachments.upsert({
        id: file.id,
        agentId: file.agentId,
        dataRecordId: file.dataRecordId,
        fileName: file.fileName,
        contentType: file.contentType,
        fileSize: file.fileSize,
        blobPath: file.blobPath,
        updatedAt: file.updatedAt,
        isDeleted: file.isDeleted,
        version: file.version
      });
    }
  }

  private generateDeviceId(): string {
    return `device_${Date.now()}_${Math.random().toString(36).substring(2, 15)}`;
  }

  async startAutoSync(intervalMs: number = 30000): Promise<void> {
    // Perform initial sync
    await this.performSync();

    // Set up periodic sync
    setInterval(async () => {
      if (navigator.onLine) {
        await this.performSync();
      }
    }, intervalMs);

    // Sync when coming back online
    window.addEventListener('online', () => {
      this.performSync();
    });
  }
}
