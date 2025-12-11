import { useEffect, useRef, useState, type ReactNode } from "react";
import { Plus, X } from "lucide-react";

import { Button } from "@ui-shadcn-components";

export interface FloatingActionMenuOption {
  label: string;
  icon?: ReactNode;
  onClick: () => void;
}

export interface FloatingActionMenuProps {
  options: FloatingActionMenuOption[];
}

/**
 * A floating action button with expandable menu options.
 *
 * Originally named `FAB` in the i-teach classroom-flow-demo repository.
 */
export const FloatingActionMenu = ({ options }: FloatingActionMenuProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const fabRef = useRef<HTMLDivElement>(null);

  useEffect(
    function handleEscapeKeyAndOutsideClick() {
      if (!isOpen) return;

      const handleEscape = (event: KeyboardEvent) => {
        if (event.key === "Escape") {
          setIsOpen(false);
        }
      };

      const handleClickOutside = (event: MouseEvent) => {
        if (fabRef.current && !fabRef.current.contains(event.target as Node)) {
          setIsOpen(false);
        }
      };

      document.addEventListener("keydown", handleEscape);
      document.addEventListener("mousedown", handleClickOutside);

      return () => {
        document.removeEventListener("keydown", handleEscape);
        document.removeEventListener("mousedown", handleClickOutside);
      };
    },
    [isOpen],
  );

  return (
    <div ref={fabRef} className="fixed bottom-6 right-6 z-40">
      {isOpen && (
        <div className="mb-4 flex flex-col gap-2 animate-in fade-in slide-in-from-bottom-2 duration-200">
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
        className="w-14 h-14 rounded-full shadow-lg border-2 border-primary transition-transform hover:scale-110"
        onClick={() => setIsOpen((previous) => !previous)}
        aria-label={isOpen ? "Close action menu" : "Open action menu"}
        aria-expanded={isOpen}
      >
        {isOpen ? <X className="w-6 h-6" /> : <Plus className="w-6 h-6" />}
      </Button>
    </div>
  );
};
