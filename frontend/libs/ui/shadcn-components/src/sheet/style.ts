export const sheetOverlayStyles =
  "fixed inset-0 z-50 bg-black/80 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0";

export const sheetCloseStyles =
  "absolute right-4 top-4 rounded-sm opacity-70 ring-offset-background transition-opacity data-[state=open]:bg-secondary hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:pointer-events-none";

export const sheetHeaderStyles = "flex flex-col space-y-2 text-center sm:text-left";

export const sheetFooterStyles = "flex flex-col-reverse sm:flex-row sm:justify-end sm:space-x-2";

export const sheetTitleStyles = "text-lg font-semibold text-foreground";

export const sheetDescriptionStyles = "text-sm text-muted-foreground";

