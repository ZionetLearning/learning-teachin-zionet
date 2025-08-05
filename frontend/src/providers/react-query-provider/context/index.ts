import { ApiService } from '@/api';
import { createContext } from 'react';

export const ReactQueryContext = createContext<ApiService | null>(null);
