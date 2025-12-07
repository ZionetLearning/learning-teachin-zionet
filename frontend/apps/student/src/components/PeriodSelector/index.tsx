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
  weekStartsOn?: 0 | 1; // 0 = Sunday, 1 = Monday
}

type ViewMode = "week" | "month";

const getStartOfWeek = (date: Date, weekStartsOn: 0 | 1 = 0): Date => {
  const d = new Date(date);
  const day = d.getDay();
  const diff = d.getDate() - day + (day < weekStartsOn ? -7 : 0) + weekStartsOn;
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
  weekStartsOn = 0,
}: Props) => {
  const { t, i18n } = useTranslation();
  const classes = useStyles();
  const [currentMonth, setCurrentMonth] = useState(new Date());
  const [viewMode, setViewMode] = useState<ViewMode>("week");

  const isSameDateRange = (newStart: Date, newEnd: Date): boolean => {
    return (
      newStart.getTime() === startDate.getTime() &&
      newEnd.getTime() === endDate.getTime()
    );
  };

  const handleModeChange = (mode: ViewMode) => {
    setViewMode(mode);
    if (mode === "week") {
      const start = getStartOfWeek(startDate, weekStartsOn);
      const end = getEndOfWeek(startDate);
      if (!isSameDateRange(start, end)) {
        onDateRangeChange(start, end);
      }
    } else {
      const start = getStartOfMonth(startDate);
      const end = getEndOfMonth(startDate);
      if (!isSameDateRange(start, end)) {
        onDateRangeChange(start, end);
      }
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
      const start = getStartOfWeek(date, weekStartsOn);
      const end = getEndOfWeek(date);
      if (!isSameDateRange(start, end)) {
        onDateRangeChange(start, end);
      }
    } else {
      const start = getStartOfMonth(date);
      const end = getEndOfMonth(date);
      if (!isSameDateRange(start, end)) {
        onDateRangeChange(start, end);
      }
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

    // Calculate offset based on week start day
    const offset = (startDay - weekStartsOn + 7) % 7;

    // Fill starting empty days
    for (let i = 0; i < offset; i++) {
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
  }, [currentMonth, weekStartsOn]);

  const isDateInRange = (date: Date | null): boolean => {
    if (!date) return false;
    return date >= startDate && date <= endDate;
  };

  const isFutureDate = (date: Date | null): boolean => {
    if (!date) return false;
    const normalizedDate = new Date(date);
    normalizedDate.setHours(0, 0, 0, 0);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return normalizedDate > today;
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

  const handleKeyDown = (
    e: React.KeyboardEvent<HTMLDivElement>,
    date: Date | null,
    weekIndex: number,
    dayIndex: number
  ) => {
    if (!date) return;

    const moveFocus = (wIdx: number, dIdx: number) => {
      const el = document.getElementById(`day-${wIdx}-${dIdx}`);
      if (el) {
        el.focus();
      }
    };

    switch (e.key) {
      case "Enter":
      case " ":
        e.preventDefault();
        if (!isFutureDate(date)) {
          handleDateClick(date);
        }
        break;
      case "ArrowRight":
        e.preventDefault();
        if (dayIndex < 6) {
          if (calendar[weekIndex][dayIndex + 1]) moveFocus(weekIndex, dayIndex + 1);
        } else if (weekIndex < calendar.length - 1) {
          if (calendar[weekIndex + 1][0]) moveFocus(weekIndex + 1, 0);
        }
        break;
      case "ArrowLeft":
        e.preventDefault();
        if (dayIndex > 0) {
          if (calendar[weekIndex][dayIndex - 1]) moveFocus(weekIndex, dayIndex - 1);
        } else if (weekIndex > 0) {
          if (calendar[weekIndex - 1][6]) moveFocus(weekIndex - 1, 6);
        }
        break;
      case "ArrowDown":
        e.preventDefault();
        if (weekIndex < calendar.length - 1) {
          if (calendar[weekIndex + 1][dayIndex]) moveFocus(weekIndex + 1, dayIndex);
        }
        break;
      case "ArrowUp":
        e.preventDefault();
        if (weekIndex > 0) {
          if (calendar[weekIndex - 1][dayIndex]) moveFocus(weekIndex - 1, dayIndex);
        }
        break;
    }
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
            {t("common.week")}
          </Button>
          <Button
            size="small"
            fullWidth
            variant={viewMode === "month" ? "contained" : "outlined"}
            color={viewMode === "month" ? "primary" : "inherit"}
            onClick={() => handleModeChange("month")}
          >
            {t("common.month")}
          </Button>
          <Button
            size="small"
            fullWidth
            variant="outlined"
            color="inherit"
            onClick={handleTodayClick}
          >
            {t("common.today")}
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

          <Box className={classes.calendar} role="grid">
            <Box className={classes.weekDays} role="row">
              {[0, 1, 2, 3, 4, 5, 6].map((weekday) => {
                const dayOffset = (weekStartsOn + weekday) % 7;
                const date = new Date(1970, 0, 4 + dayOffset); // 1970-01-04 is a Sunday
                const localizedDay = date.toLocaleDateString(i18n.language, { weekday: "short" });
                return (
                  <Typography key={weekday} variant="caption" className={classes.weekDay} role="columnheader">
                    {localizedDay}
                  </Typography>
                );
              })}
            </Box>
            {calendar.map((week, weekIndex) => (
              <Box key={weekIndex} className={classes.week} role="row">
                {week.map((date, dayIndex) => (
                  <Box
                    key={dayIndex}
                    id={`day-${weekIndex}-${dayIndex}`}
                    role="gridcell"
                    aria-label={date ? date.toLocaleDateString(undefined, { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) : ""}
                    tabIndex={date ? 0 : -1}
                    onKeyDown={(e) => handleKeyDown(e, date, weekIndex, dayIndex)}
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
