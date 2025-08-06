import { useTranslation } from "react-i18next";
import { ChatIcon } from "./icons";
import useStyles from "./style";

export const ChatHeader = () => {
  const { t } = useTranslation();
  const classes = useStyles();

  return (
    <header className={classes.header}>
      <span className={classes.title}>
        {t("pages.chatDa.learningTeachinChat")}
      </span>
      <ChatIcon width={24} height={24} />
    </header>
  );
};
