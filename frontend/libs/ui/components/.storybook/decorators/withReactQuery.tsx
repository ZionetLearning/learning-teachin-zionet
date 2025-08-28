import * as React from 'react';
import type { Decorator } from '@storybook/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

export const WithReactQuery: Decorator = (Story) => {
  const [client] = React.useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            retry: false,
            refetchOnWindowFocus: false,
          },
        },
      })
  );

  return (
    <QueryClientProvider client={client}>
      <Story />
    </QueryClientProvider>
  );
};