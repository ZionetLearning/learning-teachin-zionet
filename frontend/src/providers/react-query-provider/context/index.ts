import { createContext } from 'react';

import { ApiService } from '@/api';

export const ReactQueryContext = createContext<ApiService | null>(null);
