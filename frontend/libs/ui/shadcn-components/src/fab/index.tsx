import { useState, type ReactNode } from "react";
import { Plus, X } from "lucide-react";

import { Button } from "@ui-shadcn-components/button";

export interface FABOption {
  label: string;
  icon?: ReactNode;
  onClick: () => void;
}

export interface FABProps {
  options: FABOption[];
}

export const FAB = ({ options }: FABProps) => {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className="fixed bottom-6 right-6 z-40">
      {isOpen && (
        <div className="mb-4 flex flex-col gap-2">
          {options.map((option) => (
            <Button
              key={option.label}
              variant="secondary"
              className="justify-start gap-2 shadow-lg border-2 border-primary"
              onClick={() => {
                option.onClick();
                setIsOpen(false);
              }}
            >
              {option.icon}
              <span className="whitespace-nowrap">{option.label}</span>
            </Button>
          ))}
        </div>
      )}
      <Button
        size="lg"
        className="w-14 h-14 rounded-full shadow-lg border-2 border-primary"
        onClick={() => setIsOpen((previous) => !previous)}
      >
        {isOpen ? <X className="w-6 h-6" /> : <Plus className="w-6 h-6" />}
      </Button>
    </div>
  );
};

