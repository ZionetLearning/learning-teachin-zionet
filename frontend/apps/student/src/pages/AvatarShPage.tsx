import { useTranslation } from "react-i18next";
import { AvatarSh } from "../features";

export const AvatarShPage = () => {
  const { t } = useTranslation();

  return (
    <div>
      <h1>{t("pages.avatarSh.avatarShPage")}</h1>
      <AvatarSh />
    </div>
  );
};
