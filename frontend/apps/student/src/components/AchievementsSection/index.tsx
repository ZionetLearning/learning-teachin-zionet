import { useMemo } from "react";
import { Box, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { AchievementBadge } from "../AchievementBadge";
import type { Achievement } from "../../types/achievement";
import { useStyles } from "./style";

interface Props {
  achievements: Achievement[];
}

export const AchievementsSection = ({ achievements }: Props) => {
  const { t } = useTranslation();
  const classes = useStyles();

  const unlockedCount = useMemo(
    () => achievements.filter((a) => a.isUnlocked).length,
    [achievements],
  );

  return (
    <Box className={classes.container}>
      <Typography variant="h5" className={classes.title}>
        {t("achievements.title")}
      </Typography>
      <Typography variant="body2" className={classes.subtitle}>
        {t("achievements.progress", {
          unlocked: unlockedCount,
          total: achievements.length,
        })}
      </Typography>
      <Box className={classes.grid}>
        {achievements.map((achievement) => (
          <AchievementBadge
            key={achievement.achievementId}
            achievement={achievement}
          />
        ))}
      </Box>
    </Box>
  );
};
