import { Box, Typography, Tooltip } from "@mui/material";
import LockIcon from "@mui/icons-material/Lock";
import { getAchievementIcon } from "../../utils/achievementIcons";
import type { Achievement } from "../../types/achievement";
import { useStyles } from "./style";

interface Props {
  achievement: Achievement;
}

export const AchievementBadge = ({ achievement }: Props) => {
  const classes = useStyles({ isUnlocked: achievement.isUnlocked });
  const IconComponent = getAchievementIcon(achievement.key);

  return (
    <Tooltip title={achievement.description} arrow>
      <Box className={classes.container}>
        <Box className={classes.iconContainer}>
          <IconComponent className={classes.icon} />
          {!achievement.isUnlocked && <LockIcon className={classes.lockIcon} />}
        </Box>
        <Typography variant="caption" className={classes.name}>
          {achievement.name}
        </Typography>
        {achievement.isUnlocked && achievement.unlockedAt && (
          <Typography variant="caption" className={classes.date}>
            {new Date(achievement.unlockedAt).toLocaleDateString()}
          </Typography>
        )}
      </Box>
    </Tooltip>
  );
};
