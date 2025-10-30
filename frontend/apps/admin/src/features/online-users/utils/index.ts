import type { Theme } from "@mui/material/styles";

export const getRoleColor = (role: string | null | undefined, theme: Theme) => {
  if (!role) return theme.palette.grey[500];

  switch (role.toLowerCase()) {
    case "teacher":
      return theme.palette.primary.main;
    case "student":
      return theme.palette.success.main;
    case "admin":
      return theme.palette.warning.main;
    default:
      return theme.palette.grey[500];
  }
};

export const getRoleIcon = (role: string | null | undefined) => {
  if (!role) return "👤";

  switch (role.toLowerCase()) {
    case "teacher":
      return "👨‍🏫";
    case "student":
      return "👨‍🎓";
    case "admin":
      return "👑";
    default:
      return "👤";
  }
};
