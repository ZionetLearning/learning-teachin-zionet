export const sidebarWrapperStyles =
  "group/sidebar-wrapper flex min-h-svh w-full has-[[data-variant=inset]]:bg-sidebar";

export const sidebarBaseStyles =
  "group/sidebar relative flex h-svh w-[--sidebar-width] shrink-0 flex-col border-r bg-card text-card-foreground transition-[width] duration-300 ease-in-out data-[variant=inset]:border-border/40 data-[variant=inset]:bg-sidebar sm:duration-500";

export const sidebarContentStyles =
  "flex h-full w-full flex-1 flex-col gap-4 overflow-hidden px-2 py-4";

export const sidebarRailStyles =
  "group/sidebar-rail absolute inset-y-0 right-0 hidden w-10 flex-col items-center justify-between rounded-full border border-border/50 bg-card p-2 shadow-sm data-[state=collapsed]:flex";

export const sidebarMainStyles =
  "flex h-full flex-1 flex-col gap-6 overflow-hidden rounded-[calc(var(--radius)_-_12px)] border border-border/50 bg-card shadow-sm data-[variant=inset]:border-border/40 data-[variant=inset]:shadow-none";

export const sidebarFooterStyles =
  "flex items-center justify-between border-t border-border/50 bg-card px-2 py-2 text-sm text-muted-foreground data-[variant=inset]:border-border/40";

export const sidebarInsetStyles =
  "group/sidebar-inset flex h-svh flex-1 flex-col bg-muted/40";

export const sidebarInsetContentStyles =
  "group-[.has-mini-variant]/sidebar-wrapper:ml-[calc(var(--sidebar-width-icon)_+_1rem)] transition-[margin] duration-300 ease-in-out data-[variant=inset]:ml-0 sm:duration-500";

