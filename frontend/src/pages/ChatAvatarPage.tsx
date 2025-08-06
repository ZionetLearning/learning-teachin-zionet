import { useTranslation } from "react-i18next";
import { ChatAvatar } from "../features";

export const ChatAvatarPage = () => {
  const { t } = useTranslation();
  return (
    <div>
      <h1>{t('pages.chatAvatar.chatAvatarPage')}</h1>
      <ChatAvatar />
    </div>
  );
};
