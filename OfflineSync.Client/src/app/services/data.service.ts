import { Injectable } from '@angular/core';
import { DatabaseService, DataRecordDocument } from './database.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DataService {
  constructor(private dbService: DatabaseService) {}

  async createRecord(
    agentId: string,
    title: string,
    description: string,
    data: string
  ): Promise<void> {
    const db = this.dbService.getDatabase();
    if (!db) {
      throw new Error('Database not initialized');
    }

    await db.datarecords.insert({
      id: this.generateId().toLowerCase(),
      agentId: agentId.toLowerCase(),
      title,
      description,
      data,
      updatedAt: new Date().toISOString(),
      isDeleted: false,
      version: Date.now()
    });
  }

  async updateRecord(
    id: string,
    title: string,
    description: string,
    data: string
  ): Promise<void> {
    const db = this.dbService.getDatabase();
    if (!db) {
      throw new Error('Database not initialized');
    }

    const doc = await db.datarecords.findOne(id.toLowerCase()).exec();
    if (doc) {
      await doc.patch({
        title,
        description,
        data,
        updatedAt: new Date().toISOString(),
        version: Date.now()
      });
    }
  }

  async deleteRecord(id: string): Promise<void> {
    const db = this.dbService.getDatabase();
    if (!db) {
      throw new Error('Database not initialized');
    }

    const doc = await db.datarecords.findOne(id.toLowerCase()).exec();
    if (doc) {
      await doc.patch({
        isDeleted: true,
        updatedAt: new Date().toISOString(),
        version: Date.now()
      });
    }
  }

  getRecords$(agentId: string): Observable<DataRecordDocument[]> {
    const db = this.dbService.getDatabase();
    if (!db) {
      throw new Error('Database not initialized');
    }

    const normalizedAgentId = agentId.toLowerCase();
    console.log('[DATA] Creating records query for agentId:', normalizedAgentId);

    return db.datarecords
      .find({
        selector: {
          agentId: normalizedAgentId,
          isDeleted: false
        },
        sort: [{ updatedAt: 'desc' }]
      })
      .$ as any;
  }

  async getRecordById(id: string): Promise<DataRecordDocument | null> {
    const db = this.dbService.getDatabase();
    if (!db) {
      throw new Error('Database not initialized');
    }

    const doc = await db.datarecords.findOne(id.toLowerCase()).exec();
    return doc ? doc.toJSON() : null;
  }

  private generateId(): string {
    return Guid.newGuid();;
  }
}

class Guid {
  static newGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
      var r = Math.random() * 16 | 0,
        v = c == 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }
}