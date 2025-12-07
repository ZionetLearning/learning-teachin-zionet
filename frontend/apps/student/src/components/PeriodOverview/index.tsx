import { Box, Card, CardContent, Typography } from "@mui/material";
import {
  Bolt,
  Book,
  EmojiEvents,
  CalendarToday,
} from "@mui/icons-material";
import { useTranslation } from "react-i18next";
import type { PeriodOverviewSummary } from "../../types/summary";
import { useStyles, STAT_COLORS } from "./style";

interface Props {
  summary?: PeriodOverviewSummary;
  isLoading?: boolean;
}

const StatCard = ({
  icon,
  label,
  value,
  iconColor,
}: {
  icon: React.ReactNode;
  label: string;
  value: number | string;
  iconColor: string;
}) => {
  const classes = useStyles({ iconColor });

  return (
    <Card className={classes.statCard}>
      <CardContent className={classes.statCardContent}>
        <Box className={classes.statIconContainer}>
          {icon}
        </Box>
        <Box className={classes.statTextContainer}>
          <Typography variant="h4" className={classes.statValue}>
            {value}
          </Typography>
          <Typography variant="body2" className={classes.statLabel}>
            {label}
          </Typography>
        </Box>
      </CardContent>
    </Card>
  );
};

export const PeriodOverview = ({ summary, isLoading }: Props) => {
  const { t } = useTranslation();
  const classes = useStyles();

  const getValue = (val: number | undefined) => {
    if (isLoading || val === undefined) return "--";
    return val;
  };

  return (
    <Box className={classes.container}>
      <Typography variant="h5" className={classes.title}>
        {t("pages.summary.periodOverview")}
      </Typography>
      <Box className={classes.statsGrid}>
        <StatCard
          icon={<Bolt />}
          label={t("pages.summary.totalAttempts")}
          value={getValue(summary?.totalAttempts)}
          iconColor={STAT_COLORS.attempts}
        />
        <StatCard
          icon={<Book />}
          label={t("pages.summary.wordsLearned")}
          value={getValue(summary?.wordsLearned)}
          iconColor={STAT_COLORS.words}
        />
        <StatCard
          icon={<EmojiEvents />}
          label={t("pages.summary.achievements")}
          value={getValue(summary?.achievementsUnlocked)}
          iconColor={STAT_COLORS.achievements}
        />
        <StatCard
          icon={<CalendarToday />}
          label={t("pages.summary.practiceDays")}
          value={getValue(summary?.practiceDays)}
          iconColor={STAT_COLORS.practice}
        />
      </Box>
    </Box>
  );
};
