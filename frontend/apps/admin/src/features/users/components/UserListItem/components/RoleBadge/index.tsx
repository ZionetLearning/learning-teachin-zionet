import { AppRoleType } from "@app-providers";
import { useStyles } from "./style";
import { useTranslation } from "react-i18next";

export const RoleBadge = ({
  role,
  ...rest
}: {
  role: AppRoleType;
  "data-testid": string;
}) => {
  const classes = useStyles();
  const { t } = useTranslation();

  const cls =
    role === "student"
      ? classes.roleStudent
      : role === "teacher"
        ? classes.roleTeacher
        : classes.roleAdmin;

  return (
    <span className={`${classes.roleBadge} ${cls}`} {...rest}>
      {t(`roles.${role}`)}
    </span>
  );
};
