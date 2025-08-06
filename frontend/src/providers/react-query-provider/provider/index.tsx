import { ReactNode } from "react";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

import { ApiService } from "@/api";
import { ReactQueryContext } from "../context";

const queryClient = new QueryClient();

export const ReactQueryProvider = ({ children }: { children: ReactNode }) => {
  const apiService = new ApiService();
  return (
    <QueryClientProvider client={queryClient}>
      <ReactQueryContext.Provider value={apiService}>
        {children}
      </ReactQueryContext.Provider>
    </QueryClientProvider>
  );
};
