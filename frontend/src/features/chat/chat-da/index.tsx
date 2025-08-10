import { useState } from "react";
import { useTranslation } from "react-i18next";
import { ChatProvider } from "./providers/chat-provider";
import { FullScreenChat, SidebarChat } from "./views";

import useStyles from "./style";

export const ChatDa = () => {
  const { t } = useTranslation();
  const classes = useStyles();
  const [view, setView] = useState<"sidebar" | "full">("sidebar");
  return (
    <ChatProvider>
      <button
        className={classes.toggleButton}
        onClick={() =>
          setView((view) => (view === "sidebar" ? "full" : "sidebar"))
        }
      >
        {t("pages.chatDa.toggleView")}
      </button>
      {view === "sidebar" ? <SidebarChat /> : <FullScreenChat />}
    </ChatProvider>
  );
};
