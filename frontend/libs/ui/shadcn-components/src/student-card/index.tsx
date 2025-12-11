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
      "p-[17.875px] border cursor-pointer transition-all shadow-[0px_1px_2px_0px_rgba(0,0,0,0.05)]",
      isSelected ? "border-[#5c98fd]" : "border-[#d9d9d9]",
    )}
    onClick={(event) => {
      event.stopPropagation();
      onSelect(!isSelected);
    }}
    onDoubleClick={(event) => {
      event.stopPropagation();
      onDrillIn();
    }}
  >
    <div className="flex items-start gap-[11.98px]">
      <div className="pt-1">
        <Checkbox
          checked={isSelected}
          onCheckedChange={(checked) => onSelect(checked === true)}
          onClick={(event) => event.stopPropagation()}
          className={cn(
            "h-4 w-4 rounded",
            isSelected
              ? "bg-[#186cfb] border-[#186cfb] data-[state=checked]:bg-[#186cfb]"
              : "bg-white border-[#9ec2fe]",
          )}
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
          >
            <User className="w-4 h-4 text-[#333333]" />
            <span className="font-medium text-[13.7px] leading-5 text-[#333333] whitespace-nowrap">
              {name}
            </span>
          </button>

          {hasAlert && <AlertTriangle className="w-4 h-4 text-[#f47b25] flex-shrink-0" />}
        </div>

        <div className="flex flex-col gap-[3.99px]">
          <div className="flex items-start justify-between gap-2">
            <span className="text-[12px] leading-4 text-[#757575]">Progress</span>
            <span className="text-[12px] leading-4 text-[#808080] whitespace-nowrap">{progress}%</span>
          </div>
          <div className="w-full h-[8px] bg-[#e0e0e0] rounded-full overflow-hidden">
            <div className="h-full bg-[#186cfb] transition-all rounded-full" style={{ width: `${progress}%` }} />
          </div>
        </div>

        <div className="flex items-center gap-2">
          <div className="flex gap-px items-center h-3">
            {[0, 1, 2, 3, 4].map((value) => (
              <div key={value} className={cn("h-[10px] w-[4px] rounded-[1px]", engagementBarColor[engagement])} />
            ))}
          </div>
          <span className="text-[11.8px] leading-4 text-[#808080] capitalize">{engagement} engagement</span>
        </div>

        <div className="border-l-[1.875px] border-[#d9d9d9] pl-[9.875px]">
          <span className="text-[11.8px] leading-4 text-[#808080] whitespace-pre-wrap">{currentTask}</span>
        </div>

        <Badge
          variant="secondary"
          className="text-[12px] leading-4 font-semibold text-[#333333] bg-[#ebebeb] border-transparent px-[10.625px] py-[2.625px] rounded-full w-fit"
        >
          {personalizationMode}
        </Badge>
      </div>
    </div>
  </Card>
);

