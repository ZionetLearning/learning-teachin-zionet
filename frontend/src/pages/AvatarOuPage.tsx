import { useTranslation } from "react-i18next";
import { AvatarOu } from "../features";

export const AvatarOuPage = () => {
  const { t } = useTranslation();
  return (
    <div>
      <h1>{t("pages.avatarOu.avatarOuPage")}</h1>
      <AvatarOu />
    </div>
  );
};
