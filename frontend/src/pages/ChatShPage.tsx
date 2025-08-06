import { useTranslation } from "react-i18next";
import { ChatSh } from "../features";

export const ChatShPage = () => {
  const { t } = useTranslation();
  return (
    <div>
      <h1>{t("pages.chatSh.chatShPageWithOpenAi")}</h1>
      <ChatSh />
    </div>
  );
};
