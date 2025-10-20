import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DatabaseService, AppDatabase } from './database.service';
import { firstValueFrom, Subject } from 'rxjs';
import { environment } from '../../environments/environment';

export interface SyncRequest {
  agentId: string;
  deviceId: string;
  lastSyncVersion: number;
  pageSize: number;
  pageNumber: number;
  pushedRecords: any[];
  pushedFiles: any[];
  pushedMasterData: any[];
  pushedArticles: any[];
}

export interface SyncResponse {
  currentVersion: number;
  updatedRecords: any[];
  updatedFiles: any[];
  updatedMasterData: any[];
  updatedArticles: any[];
  totalArticles: number;
  currentPage: number;
  totalPages: number;
  hasMoreArticles: boolean;
  success: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class SyncService {
  private apiUrl = `${environment.apiUrl}/sync`;
  private agentId: string = '';
  private deviceId: string = '';
  private lastSyncVersion: number = 0;
  
  // Sync progress tracking
  private syncProgressSubject = new Subject<{ 
    issyncing: boolean; 
    progress: number; 
    message: string;
    itemsSynced: number;
    totalItems: number;
  }>();
  public syncProgress$ = this.syncProgressSubject.asObservable();

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
      this.syncProgressSubject.next({ 
        issyncing: true, 
        progress: 0, 
        message: 'Starting sync...', 
        itemsSynced: 0, 
        totalItems: 0 
      });

      // Get local changes to push to server
      const pushedRecords = await this.getLocalChanges(db);
      const pushedFiles = await this.getLocalFileChanges(db);
      const pushedMasterData = await this.getLocalMasterDataChanges(db);
      const pushedArticles = await this.getLocalArticleChanges(db);

      this.syncProgressSubject.next({ 
        issyncing: true, 
        progress: 10, 
        message: 'Syncing data...', 
        itemsSynced: 0, 
        totalItems: 0 
      });

      // Sync articles with pagination
      let currentPage = 1;
      let hasMoreArticles = true;
      let totalArticlesSynced = 0;

      while (hasMoreArticles) {
        const syncRequest: SyncRequest = {
          agentId: this.agentId,
          deviceId: this.deviceId,
          lastSyncVersion: this.lastSyncVersion,
          pageSize: 10,
          pageNumber: currentPage,
          pushedRecords: currentPage === 1 ? pushedRecords : [],
          pushedFiles: currentPage === 1 ? pushedFiles : [],
          pushedMasterData: currentPage === 1 ? pushedMasterData : [],
          pushedArticles: currentPage === 1 ? pushedArticles : []
        };

        console.log('[SYNC] Sending sync request (page ' + currentPage + '):', {
          agentId: this.agentId,
          deviceId: this.deviceId,
          lastSyncVersion: this.lastSyncVersion,
          pushedRecordsCount: syncRequest.pushedRecords.length,
          pushedFilesCount: syncRequest.pushedFiles.length,
          pushedMasterDataCount: syncRequest.pushedMasterData.length,
          pushedArticlesCount: syncRequest.pushedArticles.length,
          pageNumber: currentPage
        });

        // Send sync request to server
        const response = await firstValueFrom(
          this.http.post<SyncResponse>(`${this.apiUrl}/sync`, syncRequest)
        );

        console.log('[SYNC] Received sync response (page ' + currentPage + '):', {
          success: response.success,
          currentVersion: response.currentVersion,
          updatedRecordsCount: response.updatedRecords?.length || 0,
          updatedFilesCount: response.updatedFiles?.length || 0,
          updatedMasterDataCount: response.updatedMasterData?.length || 0,
          updatedArticlesCount: response.updatedArticles?.length || 0,
          totalArticles: response.totalArticles,
          currentPage: response.currentPage,
          totalPages: response.totalPages
        });

        if (!response.success) {
          console.error('Sync failed:', response.message);
          this.syncProgressSubject.next({ 
            issyncing: false, 
            progress: 0, 
            message: 'Sync failed', 
            itemsSynced: 0, 
            totalItems: 0 
          });
          return false;
        }

        // Apply server changes to local database
        await this.applyServerChanges(db, response);
        
        totalArticlesSynced += response.updatedArticles?.length || 0;
        
        // Update progress
        const progress = response.totalArticles > 0 
          ? Math.min(90, 20 + (totalArticlesSynced / response.totalArticles) * 70)
          : 90;
        
        this.syncProgressSubject.next({ 
          issyncing: true, 
          progress, 
          message: `Syncing articles... (${totalArticlesSynced}/${response.totalArticles})`, 
          itemsSynced: totalArticlesSynced, 
          totalItems: response.totalArticles 
        });

        hasMoreArticles = response.hasMoreArticles;
        currentPage++;

        // Update last sync version
        this.lastSyncVersion = response.currentVersion;
        localStorage.setItem(
          `lastSyncVersion_${this.agentId}_${this.deviceId}`,
          this.lastSyncVersion.toString()
        );
      }

      this.syncProgressSubject.next({ 
        issyncing: false, 
        progress: 100, 
        message: 'Sync completed', 
        itemsSynced: totalArticlesSynced, 
        totalItems: totalArticlesSynced 
      });

      console.log('Sync completed successfully');
      return true;
    } catch (error) {
      console.error('Sync error:', error);
      this.syncProgressSubject.next({ 
        issyncing: false, 
        progress: 0, 
        message: 'Sync error', 
        itemsSynced: 0, 
        totalItems: 0 
      });
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

  private async getLocalMasterDataChanges(db: AppDatabase): Promise<any[]> {
    const masterData = await db.masterdata
      .find({
        selector: {
          version: {
            $gt: this.lastSyncVersion
          }
        }
      })
      .exec();

    return masterData.map(doc => doc.toJSON());
  }

  private async getLocalArticleChanges(db: AppDatabase): Promise<any[]> {
    const articles = await db.articles
      .find({
        selector: {
          version: {
            $gt: this.lastSyncVersion
          }
        }
      })
      .exec();

    return articles.map(doc => doc.toJSON());
  }

  private async applyServerChanges(db: AppDatabase, response: SyncResponse): Promise<void> {
    console.log(`[SYNC] Applying ${response.updatedRecords?.length || 0} record(s), ${response.updatedFiles?.length || 0} file(s), ${response.updatedMasterData?.length || 0} master data item(s), and ${response.updatedArticles?.length || 0} article(s) from server`);
    
    // Apply record updates
    for (const record of response.updatedRecords || []) {
      console.log('[SYNC] Upserting record:', {
        id: record.id,
        agentId: record.agentId,
        title: record.title,
        version: record.version
      });
      const normalizedUpdatedAt = this.normalizeDateString(record.updatedAt);
      
      await db.datarecords.upsert({
        id: record.id?.toLowerCase(),
        agentId: record.agentId?.toLowerCase(),
        title: record.title,
        description: record.description,
        data: record.data,
        updatedAt: normalizedUpdatedAt,
        isDeleted: record.isDeleted,
        version: record.version
      });
    }

    // Apply file updates
    for (const file of response.updatedFiles || []) {
      const normalizedUpdatedAt = this.normalizeDateString(file.updatedAt);
      await db.fileattachments.upsert({
        id: file.id?.toLowerCase(),
        agentId: file.agentId?.toLowerCase(),
        dataRecordId: file.dataRecordId?.toLowerCase(),
        fileName: file.fileName,
        contentType: file.contentType,
        fileSize: file.fileSize,
        blobPath: file.blobPath,
        updatedAt: normalizedUpdatedAt,
        isDeleted: file.isDeleted,
        version: file.version
      });
    }

    // Apply master data updates
    for (const masterData of response.updatedMasterData || []) {
      console.log('[SYNC] Upserting master data:', {
        id: masterData.id,
        key: masterData.key,
        category: masterData.category,
        version: masterData.version
      });
      const normalizedUpdatedAt = this.normalizeDateString(masterData.updatedAt);
      await db.masterdata.upsert({
        id: masterData.id?.toLowerCase(),
        key: masterData.key,
        value: masterData.value,
        category: masterData.category,
        description: masterData.description,
        updatedAt: normalizedUpdatedAt,
        isDeleted: masterData.isDeleted,
        version: masterData.version
      });
    }

    // Apply article updates
    for (const article of response.updatedArticles || []) {
      console.log('[SYNC] Upserting article:', {
        id: article.id,
        title: article.title,
        version: article.version
      });
      const normalizedUpdatedAt = this.normalizeDateString(article.updatedAt);
      const normalizedPublishedAt = this.normalizeDateString(article.publishedAt);
      await db.articles.upsert({
        id: article.id?.toLowerCase(),
        title: article.title,
        content: article.content,
        author: article.author,
        imageData: article.imageData,
        imageContentType: article.imageContentType,
        publishedAt: normalizedPublishedAt,
        updatedAt: normalizedUpdatedAt,
        isDeleted: article.isDeleted,
        version: article.version
      });
    }
    
    console.log('[SYNC] Finished applying server changes');
  }

  private normalizeDateString(value: any): string {
    if (!value) {
      return new Date().toISOString();
    }
    try {
      const d = new Date(value);
      if (!isNaN(d.getTime())) {
        return d.toISOString();
      }
    } catch {
      // ignore
    }
    return new Date().toISOString();
  }

  private generateDeviceId(): string {
    return `device_${Date.now()}_${Math.random().toString(36).substring(2, 15)}`;
  }

  private syncIntervalId?: number;
  private onlineHandler?: () => void;

  async startAutoSync(intervalMs: number = 30000): Promise<void> {
    // Perform initial sync
    await this.performSync();

    // Set up periodic sync (store interval ID for cleanup)
    this.syncIntervalId = window.setInterval(async () => {
      if (navigator.onLine) {
        await this.performSync();
      }
    }, intervalMs);

    // Sync when coming back online (store handler for cleanup)
    this.onlineHandler = () => {
      this.performSync();
    };
    window.addEventListener('online', this.onlineHandler);
  }

  stopAutoSync(): void {
    if (this.syncIntervalId !== undefined) {
      clearInterval(this.syncIntervalId);
      this.syncIntervalId = undefined;
    }
    if (this.onlineHandler) {
      window.removeEventListener('online', this.onlineHandler);
      this.onlineHandler = undefined;
    }
  }
}
