import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DatabaseService, DataRecordDocument } from './services/database.service';
import { SyncService } from './services/sync.service';
import { DataService } from './services/data.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  title = 'Offline Sync Web App';
  agentId = '';
  isInitialized = false;
  records: DataRecordDocument[] = [];
  
  newRecord = {
    title: '',
    description: '',
    data: ''
  };
  
  isOnline = navigator.onLine;
  lastSyncTime: Date | null = null;

  constructor(
    private dbService: DatabaseService,
    private syncService: SyncService,
    private dataService: DataService
  ) {
    // Monitor online/offline status
    window.addEventListener('online', () => {
      this.isOnline = true;
    });
    
    window.addEventListener('offline', () => {
      this.isOnline = false;
    });
  }

  async ngOnInit() {
    // Try to get stored agent ID
    const storedAgentId = localStorage.getItem('agentId');
    if (storedAgentId) {
      this.agentId = storedAgentId;
      await this.initializeApp();
    }
  }

  async initializeApp() {
    if (!this.agentId) {
      alert('Please enter an Agent ID');
      return;
    }

    try {
      // Normalize agent ID to lowercase to match server format
      this.agentId = this.agentId.toLowerCase().trim();
      
      console.log('[APP] Initializing with agentId:', this.agentId);
      
      // Store agent ID
      localStorage.setItem('agentId', this.agentId);

      // Initialize database
      console.log('[APP] Initializing database...');
      await this.dbService.initDatabase(this.agentId);
      console.log('[APP] Database initialized');

      // Set up sync service
      console.log('[APP] Setting up sync service...');
      this.syncService.setAgentId(this.agentId);

      // Start auto-sync every 30 seconds
      console.log('[APP] Starting auto-sync...');
      await this.syncService.startAutoSync(30000);
      console.log('[APP] Auto-sync started');

      // Subscribe to records
      console.log('[APP] Setting up records subscription...');
      this.dataService.getRecords$(this.agentId).subscribe(records => {
        console.log('[APP] Records subscription emitted:', records.length, 'records');
        this.records = records;
      });
      console.log('[APP] Records subscription set up');

      this.isInitialized = true;
      this.lastSyncTime = new Date();
      console.log('[APP] Initialization complete');
    } catch (error) {
      console.error('Initialization error:', error);
      alert('Failed to initialize app. Please check console for details.');
    }
  }

  async createNewRecord() {
    if (!this.newRecord.title) {
      alert('Title is required');
      return;
    }

    try {
      await this.dataService.createRecord(
        this.agentId,
        this.newRecord.title,
        this.newRecord.description,
        this.newRecord.data
      );

      // Clear form
      this.newRecord = {
        title: '',
        description: '',
        data: ''
      };

      // Trigger sync
      await this.syncService.performSync();
      this.lastSyncTime = new Date();
    } catch (error) {
      console.error('Error creating record:', error);
      alert('Failed to create record');
    }
  }

  async deleteRecord(id: string) {
    if (confirm('Are you sure you want to delete this record?')) {
      try {
        await this.dataService.deleteRecord(id);
        await this.syncService.performSync();
        this.lastSyncTime = new Date();
      } catch (error) {
        console.error('Error deleting record:', error);
        alert('Failed to delete record');
      }
    }
  }

  async manualSync() {
    try {
      const success = await this.syncService.performSync();
      if (success) {
        this.lastSyncTime = new Date();
        alert('Sync completed successfully');
      } else {
        alert('Sync failed. Please check console for details.');
      }
    } catch (error) {
      console.error('Sync error:', error);
      alert('Sync failed');
    }
  }
}
