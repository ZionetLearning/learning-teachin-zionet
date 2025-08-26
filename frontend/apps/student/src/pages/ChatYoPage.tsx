import { useTranslation } from "react-i18next";
import { ChatYo } from "../features";

export const ChatYoPage = () => {
  const { t } = useTranslation();

  return (
    <div>
      <h1>{t("pages.chatYo.chatYoPageOpenAI")}</h1>
      <ChatYo />
    </div>
  );
};
