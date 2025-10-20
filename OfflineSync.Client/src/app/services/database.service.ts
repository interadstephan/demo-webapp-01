import { Injectable } from '@angular/core';
import { createRxDatabase, addRxPlugin, RxDatabase, RxCollection } from 'rxdb';
import { getRxStorageDexie } from 'rxdb/plugins/storage-dexie';
import { RxDBDevModePlugin } from 'rxdb/plugins/dev-mode';
import { wrappedValidateAjvStorage } from 'rxdb/plugins/validate-ajv';
import { RxDBMigrationSchemaPlugin } from 'rxdb/plugins/migration-schema';

// Add RxDB plugins
addRxPlugin(RxDBDevModePlugin);
addRxPlugin(RxDBMigrationSchemaPlugin);

// Define schema types
export interface DataRecordDocument {
  id: string;
  agentId: string;
  title: string;
  description: string;
  data: string;
  updatedAt: string;
  isDeleted: boolean;
  version: number;
}

export interface FileAttachmentDocument {
  id: string;
  agentId: string;
  dataRecordId?: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  blobPath: string;
  updatedAt: string;
  isDeleted: boolean;
  version: number;
}

export interface MasterDataDocument {
  id: string;
  key: string;
  value: string;
  category: string;
  description: string;
  updatedAt: string;
  isDeleted: boolean;
  version: number;
}

// Define database collections
export type DatabaseCollections = {
  datarecords: RxCollection<DataRecordDocument>;
  fileattachments: RxCollection<FileAttachmentDocument>;
  masterdata: RxCollection<MasterDataDocument>;
};

export type AppDatabase = RxDatabase<DatabaseCollections>;

@Injectable({
  providedIn: 'root'
})
export class DatabaseService {
  private db: AppDatabase | null = null;

  async initDatabase(agentId: string): Promise<AppDatabase> {
    if (this.db) {
      return this.db;
    }

    // Create database
    this.db = await createRxDatabase<DatabaseCollections>({
      name: `offlinedb_${agentId}`,
      storage: wrappedValidateAjvStorage({
        storage: getRxStorageDexie()
      }),
      ignoreDuplicate: true
    });

    // Add collections
    await this.db.addCollections({
      datarecords: {
        schema: {
          version: 2,
          primaryKey: 'id',
          type: 'object',
          properties: {
            id: {
              type: 'string',
              maxLength: 100
            },
            agentId: {
              type: 'string',
              maxLength: 100
            },
            title: {
              type: 'string'
            },
            description: {
              type: 'string'
            },
            data: {
              type: 'string'
            },
            updatedAt: {
              type: 'string',
              maxLength: 50
            },
            isDeleted: {
              type: 'boolean'
            },
            version: {
              type: 'number',
              multipleOf: 1,
              minimum: 0,
              maximum: 17609481713130
            }
          },
          required: ['id', 'agentId', 'title', 'updatedAt', 'version'],
          indexes: ['agentId', 'updatedAt', 'version']
        },
        migrationStrategies: {
          1: (oldDoc: any) => oldDoc,
          2: (oldDoc: any) => oldDoc
        }
      },
      fileattachments: {
        schema: {
          version: 2,
          primaryKey: 'id',
          type: 'object',
          properties: {
            id: {
              type: 'string',
              maxLength: 100
            },
            agentId: {
              type: 'string',
              maxLength: 100
            },
            dataRecordId: {
              type: 'string',
              maxLength: 100
            },
            fileName: {
              type: 'string'
            },
            contentType: {
              type: 'string'
            },
            fileSize: {
              type: 'number'
            },
            blobPath: {
              type: 'string'
            },
            updatedAt: {
              type: 'string',
              maxLength: 50
            },
            isDeleted: {
              type: 'boolean'
            },
            version: {
              type: 'number',
              multipleOf: 1,
              minimum: 0,
              maximum: 17609481713130
            }
          },
          required: ['id', 'agentId', 'fileName', 'updatedAt', 'version'],
          indexes: ['agentId', 'updatedAt', 'version']
        },
        migrationStrategies: {
          1: (oldDoc: any) => oldDoc,
          2: (oldDoc: any) => oldDoc
        }
      },
      masterdata: {
        schema: {
          version: 0,
          primaryKey: 'id',
          type: 'object',
          properties: {
            id: {
              type: 'string',
              maxLength: 100
            },
            key: {
              type: 'string'
            },
            value: {
              type: 'string'
            },
            category: {
              type: 'string'
            },
            description: {
              type: 'string'
            },
            updatedAt: {
              type: 'string',
              maxLength: 50
            },
            isDeleted: {
              type: 'boolean'
            },
            version: {
              type: 'number',
              multipleOf: 1,
              minimum: 0,
              maximum: 17609481713130
            }
          },
          required: ['id', 'key', 'updatedAt', 'version'],
          indexes: ['category', 'updatedAt', 'version']
        }
      }
    });

    return this.db;
  }

  getDatabase(): AppDatabase | null {
    return this.db;
  }

  async destroyDatabase(): Promise<void> {
    if (this.db) {
      await this.db.remove();
      this.db = null;
    }
  }
}
