import { useTranslation } from "react-i18next";
import { ChatWithAvatar } from "../features";

export const ChatWithAvatarPage = () => {
  const { t } = useTranslation();
  return (
    <div>
      <h1>{t("pages.chatAvatar.chatAvatarPage")}</h1>
      <ChatWithAvatar />
    </div>
  );
};
