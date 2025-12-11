import { type ReactNode } from "react";
import { X } from "lucide-react";

import { Button } from "@ui-shadcn-components";
import { cn } from "@ui-shadcn-components/utils";

export interface SideDetailPanelProps {
  children: ReactNode;
  className?: string;
}

export interface SideDetailPanelHeaderProps {
  title: string;
  description?: string;
  onClose?: () => void;
}

/**
 * A side panel component container for displaying detailed information with composable content.
 *
 * Originally named `DrillInPanel` in the i-teach classroom-flow-demo repository.
 *
 * @example
 * ```tsx
 * <SideDetailPanel>
 *   <SideDetailPanel.Header
 *     title="Control Panel"
 *     description="Manage students and lessons"
 *     onClose={handleClose}
 *   />
 *   <Tabs defaultValue="alerts">
 *     <TabsList>
 *       <TabsTrigger value="alerts">Alerts</TabsTrigger>
 *     </TabsList>
 *     <TabsContent value="alerts">
 *       <YourCustomContent />
 *     </TabsContent>
 *   </Tabs>
 * </SideDetailPanel>
 * ```
 */
export const SideDetailPanel = ({ children, className }: SideDetailPanelProps) => {
  return (
    <div
      className={cn(
        "w-full md:w-96 bg-card border-l-2 border-primary shadow-xl overflow-y-auto flex-shrink-0",
        className,
      )}
    >
      {children}
    </div>
  );
};

/**
 * Header component for SideDetailPanel with title, optional description, and close button.
 */
export const SideDetailPanelHeader = ({
  title,
  description,
  onClose,
}: SideDetailPanelHeaderProps) => {
  return (
    <div className="sticky top-0 bg-card border-b-2 border-border p-4 z-10">
      <div className="flex items-center justify-between mb-2">
        <h2 className="text-lg font-bold">{title}</h2>
        {onClose && (
          <Button
            variant="ghost"
            size="sm"
            onClick={onClose}
            title="Close panel"
            aria-label="Close panel"
          >
            <X className="w-4 h-4" />
          </Button>
        )}
      </div>
      {description && (
        <p className="text-xs text-muted-foreground">{description}</p>
      )}
    </div>
  );
};

SideDetailPanel.Header = SideDetailPanelHeader;
