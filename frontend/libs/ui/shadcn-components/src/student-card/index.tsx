import { AlertTriangle, User } from "lucide-react";

import { Badge } from "@ui-shadcn-components/badge";
import { Card } from "@ui-shadcn-components/card";
import { Checkbox } from "@ui-shadcn-components/checkbox";
import { cn } from "@ui-shadcn-components/utils";

type EngagementLevel = "high" | "medium" | "low";
type PersonalizationMode = "Auto" | "Override";

export interface StudentCardProps {
  name: string;
  progress: number;
  engagement: EngagementLevel;
  hasAlert: boolean;
  currentTask: string;
  personalizationMode: PersonalizationMode;
  isSelected: boolean;
  onSelect: (selected: boolean) => void;
  onDrillIn: () => void;
}

const engagementBarColor: Record<EngagementLevel, string> = {
  high: "bg-[rgba(22,140,41,0.6)]",
  medium: "bg-[rgba(255,193,7,0.6)]",
  low: "bg-[rgba(220,53,69,0.6)]",
};

export const StudentCard = ({
  name,
  progress,
  engagement,
  hasAlert,
  currentTask,
  personalizationMode,
  isSelected,
  onSelect,
  onDrillIn,
}: StudentCardProps) => (
  <Card
    className={cn(
      "p-4 border cursor-pointer transition-all shadow-sm",
      isSelected ? "border-primary" : "border-border",
    )}
    onClick={(event) => {
      event.stopPropagation();
      onSelect(!isSelected);
    }}
  >
    <div className="flex items-start gap-3">
      <div className="pt-1">
        <Checkbox
          checked={isSelected}
          onCheckedChange={(checked) => onSelect(checked === true)}
          onClick={(event) => event.stopPropagation()}
          className="h-4 w-4 rounded"
        />
      </div>

      <div className="flex-1 min-w-0 flex flex-col gap-2">
        <div className="flex items-center justify-between">
          <button
            type="button"
            className="flex items-center gap-2 cursor-pointer"
            onClick={(event) => {
              event.stopPropagation();
              onDrillIn();
            }}
            aria-label={`View ${name} details`}
          >
            <User className="w-4 h-4 text-foreground" />
            <span className="font-medium text-sm leading-5 text-foreground whitespace-nowrap">
              {name}
            </span>
          </button>

          {hasAlert && <AlertTriangle className="w-4 h-4 text-orange-500 flex-shrink-0" />}
        </div>

        <div className="flex flex-col gap-1">
          <div className="flex items-start justify-between gap-2">
            <span className="text-xs leading-4 text-muted-foreground">Progress</span>
            <span className="text-xs leading-4 text-muted-foreground whitespace-nowrap">{progress}%</span>
          </div>
          <div className="w-full h-2 bg-muted rounded-full overflow-hidden">
            <div className="h-full bg-primary transition-all rounded-full" style={{ width: `${progress}%` }} />
          </div>
        </div>

        <div className="flex items-center gap-2" aria-label={`Engagement level: ${engagement}`}>
          <div className="flex gap-px items-center h-3">
            {[0, 1, 2, 3, 4].map((value) => (
              <div key={value} className={cn("h-[10px] w-[4px] rounded-[1px]", engagementBarColor[engagement])} />
            ))}
          </div>
          <span className="text-xs leading-4 text-muted-foreground capitalize">{engagement} engagement</span>
        </div>

        <div className="border-l-2 border-border pl-2.5">
          <span className="text-xs leading-4 text-muted-foreground whitespace-pre-wrap">{currentTask}</span>
        </div>

        <Badge
          variant="secondary"
          className="text-xs leading-4 font-semibold px-2.5 py-0.5 rounded-full w-fit"
        >
          {personalizationMode}
        </Badge>
      </div>
    </div>
  </Card>
);

