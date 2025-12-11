import { Box, Typography, CircularProgress } from "@mui/material";
import { useTranslation } from "react-i18next";
import { useGetPeriodAchievements } from "@student/api";
import { AchievementsSection } from "@student/components";
import type { Achievement } from "@student/types/achievement";
import { useStyles } from "./style";

interface Props {
  userId: string;
  startDate: string;
  endDate: string;
}

export const AchievementsSummary = ({ userId, startDate, endDate }: Props) => {
  const { t } = useTranslation();
  const classes = useStyles();

  const { data, isLoading } = useGetPeriodAchievements({
    userId,
    startDate,
    endDate,
  });

  if (isLoading) {
    return (
      <Box className={classes.sectionShell}>
        <Box className={classes.centerContent}>
          <CircularProgress />
        </Box>
      </Box>
    );
  }

  const achievements = data?.unlockedInPeriod || [];

  if (achievements.length === 0) {
    return (
      <Box className={classes.sectionShell}>
        <Box className={classes.centerContent}>
          <Typography variant="h6" color="text.secondary">
            {t("pages.summary.achievementsSummary.noAchievements")}
          </Typography>
        </Box>
      </Box>
    );
  }

  const normalizedAchievements: Achievement[] = achievements.map((achievement) => ({
    achievementId: achievement.achievementId,
    key: achievement.feature?.toLowerCase() ?? achievement.achievementId,
    name: achievement.name,
    description: achievement.description,
    type: achievement.feature,
    feature: achievement.feature,
    targetCount: 0,
    isUnlocked: true,
    unlockedAt: achievement.unlockedAt,
  }));

  return <AchievementsSection achievements={normalizedAchievements} />;
};
