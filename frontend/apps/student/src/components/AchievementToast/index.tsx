import { Box, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { getAchievementIcon } from "../../utils/achievementIcons";
import type { AchievementUnlockedNotification } from "../../types/achievement";
import { useStyles } from "./style";

interface Props {
  achievement: AchievementUnlockedNotification;
}

export const AchievementToast = ({ achievement }: Props) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const IconComponent = getAchievementIcon(achievement.key);

  return (
    <Box className={classes.container}>
      <IconComponent className={classes.icon} />
      <Box>
        <Typography variant="h6" className={classes.title}>
          {t("achievements.unlocked")}
        </Typography>
        <Typography variant="body1" className={classes.name}>
          {achievement.name}
        </Typography>
        <Typography variant="body2" className={classes.description}>
          {achievement.description}
        </Typography>
      </Box>
    </Box>
  );
};
