import { useTranslation } from "react-i18next";
import { useStyles } from "./style";

import { UsersCreationForm, UsersTable } from "./components";

export const Users = () => {
  const { i18n } = useTranslation();
  const classes = useStyles();
  const dir = i18n.dir();
  const isRtl = dir === "rtl";

  return (
    <div className={classes.root} data-testid="users-page">
      <UsersCreationForm isRtl={isRtl} />
      <UsersTable dir={dir} />
    </div>
  );
};
