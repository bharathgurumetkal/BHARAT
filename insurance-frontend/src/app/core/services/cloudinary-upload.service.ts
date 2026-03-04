import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CloudinaryUploadService {
  // Use environment variables for these placeholders
private cloudName = 'dfwj0qzvz';
private uploadPreset = 'insurance_claim_upload';
private folder = 'claims';
private uploadUrl = `https://api.cloudinary.com/v1_1/${this.cloudName}/raw/upload`;

  constructor(private http: HttpClient) {}

  /**
   * Upload multiple files to Cloudinary and return their secure URLs
   */
  async uploadFiles(files: File[]): Promise<string[]> {
    if (!files || files.length === 0) return [];
    
    this.validateFiles(files);

    const uploadPromises = files.map(file => this.uploadSingleFile(file));
    return Promise.all(uploadPromises);
  }

  /**
   * Upload a single file to Cloudinary
   */
  private async uploadSingleFile(file: File): Promise<string> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('upload_preset', this.uploadPreset);
    formData.append('folder', this.folder);

    try {
      const response = await firstValueFrom(
        this.http.post<any>(this.uploadUrl, formData)
      );
      
      if (response && response.secure_url) {
        return response.secure_url;
      }
      throw new Error('Upload failed: secure_url not found in response');
    } catch (error) {
      console.error('Cloudinary upload error:', error);
      throw new Error(`Failed to upload file: ${file.name}`);
    }
  }

  /**
   * Validate file count and size
   */
  private validateFiles(files: File[]): void {
    if (files.length > 5) {
      throw new Error('Maximum 5 files allowed.');
    }

    const maxSize = 5 * 1024 * 1024; // 5MB
    for (const file of files) {
      if (file.size > maxSize) {
        throw new Error(`File ${file.name} exceeds the 5MB size limit.`);
      }
      
      const allowedTypes = [
        'application/pdf', 
        'image/jpeg', 
        'image/png', 
        'application/msword', 
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
      ];
      
      if (!allowedTypes.includes(file.type)) {
        // Fallback for some OS/Browser mime detection
        const extension = file.name.split('.').pop()?.toLowerCase();
        const allowedExtensions = ['pdf', 'jpg', 'jpeg', 'png', 'doc', 'docx'];
        if (!extension || !allowedExtensions.includes(extension)) {
             throw new Error(`File ${file.name} has an unsupported format.`);
        }
      }
    }
  }
}
