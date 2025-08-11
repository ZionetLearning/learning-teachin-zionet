import { useTranslation } from "react-i18next";
import { ChatWithAvatar } from "@/features";

export const ChatWithAvatarPage = () => {
  const { t } = useTranslation();
  return (
    <div>
      <h1>{t("pages.chatSh.chatShPageWithOpenAi")}</h1>
      <ChatWithAvatar />
    </div>
  );
};
