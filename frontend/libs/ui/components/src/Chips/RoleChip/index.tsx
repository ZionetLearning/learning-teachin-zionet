import { AppRoleType } from "@app-providers";
import { useTranslation } from "react-i18next";
import { useStyles } from "./style";

export const RoleChip = ({
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
