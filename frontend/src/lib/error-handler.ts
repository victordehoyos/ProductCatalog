import { AxiosError } from 'axios';

interface ApiError {
  error?: string;
  message?: string;
}

export function getErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    const errorData = error.response?.data as ApiError;
    return errorData?.error || errorData?.message || error.message || 'An error occurred';
  }
  
  if (error instanceof Error) {
    return error.message;
  }
  
  if (typeof error === 'string') {
    return error;
  }
  
  return 'An unexpected error occurred';
}

export function isAxiosError(error: unknown): error is AxiosError {
  return error instanceof AxiosError;
}

export function isError(error: unknown): error is Error {
  return error instanceof Error;
}