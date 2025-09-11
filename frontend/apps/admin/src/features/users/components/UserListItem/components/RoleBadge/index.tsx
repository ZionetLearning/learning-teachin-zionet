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

  const roleClassMap = {
    student: classes.roleStudent,
    admin: classes.roleAdmin,
    teacher: classes.roleTeacher,
  };

  const cls = roleClassMap[role] || classes.roleTeacher;

  return (
    <span className={`${classes.roleBadge} ${cls}`} {...rest}>
      {t(`roles.${role}`)}
    </span>
  );
};
