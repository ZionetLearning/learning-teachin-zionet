import { useEffect, useMemo, useState } from "react";
import { DevToolsContext } from "../../context";
import { useTranslation } from "react-i18next";

export const DevToolsProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const { i18n } = useTranslation();
  const isHebrew = i18n.language === "he";

  const [isOpen, setIsOpen] = useState(false);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.ctrlKey && e.key === "`") {
        e.preventDefault();
        setIsOpen((o) => !o);
      }
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, []);

  const value = useMemo(
    () => ({ isOpen, setOpen: setIsOpen, isHebrew }),
    [isOpen, isHebrew],
  );

  return (
    <DevToolsContext.Provider value={value}>
      {children}
    </DevToolsContext.Provider>
  );
};
