import { useState, useMemo } from "react";
import {
  Box,
  Typography,
  Button,
  IconButton,
} from "@mui/material";
import { ChevronLeft, ChevronRight } from "@mui/icons-material";
import { useTranslation } from "react-i18next";
import { useStyles } from "./style";

interface Props {
  startDate: Date;
  endDate: Date;
  onDateRangeChange: (startDate: Date, endDate: Date) => void;
}

type ViewMode = "week" | "month";

const getStartOfWeek = (date: Date): Date => {
  const d = new Date(date);
  const day = d.getDay();
  const diff = d.getDate() - day;
  return new Date(d.setDate(diff));
};

const getEndOfWeek = (date: Date): Date => {
  const start = getStartOfWeek(date);
  const end = new Date(start);
  end.setDate(start.getDate() + 6);
  return end;
};

const getStartOfMonth = (date: Date): Date => {
  return new Date(date.getFullYear(), date.getMonth(), 1);
};

const getEndOfMonth = (date: Date): Date => {
  return new Date(date.getFullYear(), date.getMonth() + 1, 0);
};

export const PeriodSelector = ({
  startDate,
  endDate,
  onDateRangeChange,
}: Props) => {
  const { t, i18n } = useTranslation();
  const classes = useStyles();
  const [currentMonth, setCurrentMonth] = useState(new Date());
  const [viewMode, setViewMode] = useState<ViewMode>("week");

  const handleModeChange = (mode: ViewMode) => {
    setViewMode(mode);
    if (mode === "week") {
      const start = getStartOfWeek(startDate);
      const end = getEndOfWeek(startDate);
      onDateRangeChange(start, end);
    } else {
      const start = getStartOfMonth(startDate);
      const end = getEndOfMonth(startDate);
      onDateRangeChange(start, end);
    }
  };

  const handleMonthChange = (direction: "prev" | "next") => {
    const newMonth = new Date(currentMonth);
    if (direction === "prev") {
      newMonth.setMonth(currentMonth.getMonth() - 1);
    } else {
      newMonth.setMonth(currentMonth.getMonth() + 1);
    }
    setCurrentMonth(newMonth);
  };

  const handleDateClick = (date: Date) => {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    // Don't allow selecting future dates
    if (date > today) return;
    
    if (viewMode === "week") {
      const start = getStartOfWeek(date);
      const end = getEndOfWeek(date);
      onDateRangeChange(start, end);
    } else {
      const start = getStartOfMonth(date);
      const end = getEndOfMonth(date);
      onDateRangeChange(start, end);
    }
  };

  const calendar = useMemo(() => {
    const year = currentMonth.getFullYear();
    const month = currentMonth.getMonth();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const startDay = firstDay.getDay();
    const daysInMonth = lastDay.getDate();

    const weeks: (Date | null)[][] = [];
    let currentWeek: (Date | null)[] = [];

    // Fill starting empty days
    for (let i = 0; i < startDay; i++) {
      currentWeek.push(null);
    }

    // Fill days of month
    for (let day = 1; day <= daysInMonth; day++) {
      currentWeek.push(new Date(year, month, day));
      if (currentWeek.length === 7) {
        weeks.push(currentWeek);
        currentWeek = [];
      }
    }

    // Fill ending empty days
    if (currentWeek.length > 0) {
      while (currentWeek.length < 7) {
        currentWeek.push(null);
      }
      weeks.push(currentWeek);
    }

    return weeks;
  }, [currentMonth]);

  const isDateInRange = (date: Date | null): boolean => {
    if (!date) return false;
    return date >= startDate && date <= endDate;
  };

  const isFutureDate = (date: Date | null): boolean => {
    if (!date) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return date > today;
  };

  const isToday = (date: Date | null): boolean => {
    if (!date) return false;
    const today = new Date();
    return (
      date.getDate() === today.getDate() &&
      date.getMonth() === today.getMonth() &&
      date.getFullYear() === today.getFullYear()
    );
  };

  const handleTodayClick = () => {
    const today = new Date();
    setCurrentMonth(today);
    handleDateClick(today);
  };

  const formatDateRange = () => {
    const options: Intl.DateTimeFormatOptions = {
      month: "short",
      day: "numeric",
      year: "numeric",
    };
    return `${startDate.toLocaleDateString(undefined, options)} - ${endDate.toLocaleDateString(undefined, options)}`;
  };

  return (
    <Box className={classes.card}>
      <Typography variant="body2" className={classes.dateRange}>
        {formatDateRange()}
      </Typography>

      <Box className={classes.contentLayout}>
        <Box className={classes.buttonGroup}>
          <Button
            size="small"
            fullWidth
            variant={viewMode === "week" ? "contained" : "outlined"}
            color={viewMode === "week" ? "primary" : "inherit"}
            onClick={() => handleModeChange("week")}
          >
            {t("Week")}
          </Button>
          <Button
            size="small"
            fullWidth
            variant={viewMode === "month" ? "contained" : "outlined"}
            color={viewMode === "month" ? "primary" : "inherit"}
            onClick={() => handleModeChange("month")}
          >
            {t("Month")}
          </Button>
          <Button
            size="small"
            fullWidth
            variant="outlined"
            color="inherit"
            onClick={handleTodayClick}
          >
            {t("Today")}
          </Button>
        </Box>

        <Box className={classes.calendarSection}>
          <Box className={classes.calendarHeader}>
            <IconButton 
              size="small" 
              onClick={() => handleMonthChange(i18n.dir() === "rtl" ? "next" : "prev")}
            >
              {i18n.dir() === "rtl" ? (
                <ChevronRight fontSize="small" />
              ) : (
                <ChevronLeft fontSize="small" />
              )}
            </IconButton>
            <Typography variant="subtitle1" className={classes.monthYear}>
              {currentMonth.toLocaleDateString(undefined, {
                month: "long",
                year: "numeric",
              })}
            </Typography>
            <IconButton 
              size="small" 
              onClick={() => handleMonthChange(i18n.dir() === "rtl" ? "prev" : "next")}
            >
              {i18n.dir() === "rtl" ? (
                <ChevronLeft fontSize="small" />
              ) : (
                <ChevronRight fontSize="small" />
              )}
            </IconButton>
          </Box>

          <Box className={classes.calendar}>
            <Box className={classes.weekDays}>
              {["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"].map((day) => (
                <Typography key={day} variant="caption" className={classes.weekDay}>
                  {day}
                </Typography>
              ))}
            </Box>
            {calendar.map((week, weekIndex) => (
              <Box key={weekIndex} className={classes.week}>
                {week.map((date, dayIndex) => (
                  <Box
                    key={dayIndex}
                    className={`${classes.day} ${
                      date && isDateInRange(date) ? classes.selectedDay : ""
                    } ${date && isToday(date) ? classes.today : ""} ${!date ? classes.emptyDay : ""} ${
                      date && isFutureDate(date) ? classes.futureDay : ""
                    }`}
                    onClick={() => date && !isFutureDate(date) && handleDateClick(date)}
                  >
                    {date ? date.getDate() : ""}
                  </Box>
                ))}
              </Box>
            ))}
          </Box>
        </Box>
      </Box>
    </Box>
  );
};
