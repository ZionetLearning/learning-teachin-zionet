import { useMemo, useState } from "react";
import {
  Avatar,
  Box,
  Chip,
  Divider,
  List,
  ListItem,
  ListItemAvatar,
  ListItemButton,
  ListItemText,
  Paper,
  Typography,
} from "@mui/material";
import { useTranslation } from "react-i18next";

import { useMyClasses, type Member, type ClassItem } from "@api";
import { getInitials } from "./utils";
import { useStyles } from "./style";

export const Classes = () => {
  const { t, i18n } = useTranslation();
  const isRTL = i18n.dir() === "rtl";
  const classes = useStyles();

  const { data, isLoading, isError, isFetching } = useMyClasses();
  const [selectedClassId, setSelectedClassId] = useState<string | undefined>();

  const selectedClass: ClassItem | undefined = useMemo(() => {
    if (!data || !selectedClassId) return undefined;
    return data.find((c) => c.classId === selectedClassId);
  }, [data, selectedClassId]);

  // split by role (0=Student, 1=Teacher)
  const { teachers, students } = useMemo(() => {
    const none = { teachers: [] as Member[], students: [] as Member[] };
    if (!selectedClass?.members) return none;
    return {
      teachers: selectedClass.members.filter((m) => m.role === 1),
      students: selectedClass.members.filter((m) => m.role === 0),
    };
  }, [selectedClass]);

  if (isLoading) {
    return (
      <Box className={classes.rootWrapper}>
        <Paper className={classes.panel}>
          <Box className={classes.centerState}>
            <Typography variant="h6">
              {t("common.loading") || "Loading…"}
            </Typography>
            <Typography variant="body2" className={classes.subtle}>
              {t("pages.classes.my.loadingHint") ||
                "Fetching your classes and members"}
            </Typography>
            <div className={classes.updatingLine} />
          </Box>
        </Paper>
      </Box>
    );
  }

  if (isError) {
    return (
      <Box className={classes.rootWrapper}>
        <Paper className={classes.panel}>
          <Box className={classes.centerState}>
            <Typography color="error" fontWeight={700}>
              {t("common.error") || "Failed to load classes."}
            </Typography>
            <Typography variant="body2" className={classes.subtle}>
              {t("common.tryAgain") || "Please try again."}
            </Typography>
          </Box>
        </Paper>
      </Box>
    );
  }

  if (!data || data.length === 0) {
    return (
      <Box className={classes.rootWrapper}>
        <Paper className={classes.panel}>
          <Box className={classes.centerState}>
            <Typography variant="h6">
              {t("pages.classes.my.emptyTitle") || "No classes yet"}
            </Typography>
            <Typography variant="body2" className={classes.subtle}>
              {t("pages.classes.my.emptySubtitle") ||
                "Create your first class to get started"}
            </Typography>
          </Box>
        </Paper>
      </Box>
    );
  }

  return (
    <Box className={classes.rootWrapper}>
      <Box className={classes.root} dir={isRTL ? "rtl" : "ltr"}>
        {/* LEFT: classes list */}
        <Paper className={classes.sidebar}>
          <Box className={classes.sidebarHeader} dir={isRTL ? "rtl" : "ltr"}>
            <Typography className={classes.sidebarTitle}>
              {t("pages.classes.my.title") || "My Classes"}
            </Typography>
            <Chip
              size="small"
              label={data.length}
              className={classes.countChip}
            />
          </Box>

          <List disablePadding className={classes.list}>
            {data.map((c) => {
              const selected = c.classId === selectedClassId;
              return (
                <ListItem disablePadding key={c.classId}>
                  <ListItemButton
                    className={classes.listItem}
                    selected={selected}
                    onClick={() => setSelectedClassId(c.classId)}
                  >
                    <ListItemText
                      primary={c.name}
                      secondary={
                        isFetching && selected
                          ? t("common.updating") || "Updating…"
                          : undefined
                      }
                    />
                  </ListItemButton>
                </ListItem>
              );
            })}
          </List>
        </Paper>

        {/* RIGHT: details */}
        <Paper className={classes.panel}>
          {!selectedClass ? (
            <Box className={classes.centerState}>
              <Typography variant="h6">
                {t("pages.classes.my.selectPrompt") || "Select a class"}
              </Typography>
              <Typography variant="body2" className={classes.subtle}>
                {t("pages.classes.my.selectHint") ||
                  "Members and roles will appear here"}
              </Typography>
            </Box>
          ) : (
            <>
              <Box className={classes.headerRow} dir={isRTL ? "rtl" : "ltr"}>
                <Typography className={classes.className}>
                  {selectedClass.name}
                </Typography>
                <Chip
                  size="small"
                  className={classes.countChip}
                  label={`${selectedClass.members?.length ?? 0} ${t("pages.classes.my.members") || "members"}`}
                />
              </Box>

              <Divider className={classes.divider} />

              {selectedClass.members.length === 0 ? (
                <Typography variant="body2" className={classes.emptyNote}>
                  {t("pages.classes.my.noMembers") ||
                    "No members in this class yet."}
                </Typography>
              ) : (
                <Box className={classes.sectionGrid}>
                  {/* Teachers */}
                  <Box className={classes.sectionCard}>
                    <Box
                      className={classes.sectionHeader}
                      dir={isRTL ? "rtl" : "ltr"}
                    >
                      <Typography className={classes.sectionTitle}>
                        {t("pages.classes.my.teachers") || "Teachers"}
                      </Typography>
                      <span className={classes.countChip}>
                        {teachers.length}
                      </span>
                    </Box>
                    <List dense className={classes.memberList}>
                      {teachers.map((m) => (
                        <ListItem key={m.memberId}>
                          <ListItemAvatar>
                            <Avatar
                              className={classes.memberAvatar}
                              dir={isRTL ? "rtl" : "ltr"}
                            >
                              {getInitials(m.name)}
                            </Avatar>
                          </ListItemAvatar>
                          <ListItemText
                            primary={
                              <span className={classes.memberName}>
                                {m.name}
                              </span>
                            }
                          />
                        </ListItem>
                      ))}
                      {teachers.length === 0 && (
                        <Typography
                          variant="body2"
                          className={classes.emptyNote}
                        >
                          {t("pages.classes.my.noTeachers") || "No teachers."}
                        </Typography>
                      )}
                    </List>
                  </Box>

                  {/* Students */}
                  <Box className={classes.sectionCard}>
                    <Box
                      className={classes.sectionHeader}
                      dir={isRTL ? "rtl" : "ltr"}
                    >
                      <Typography className={classes.sectionTitle}>
                        {t("pages.classes.my.students") || "Students"}
                      </Typography>
                      <span className={classes.countChip}>
                        {students.length}
                      </span>
                    </Box>
                    <List dense className={classes.memberList}>
                      {students.map((m) => (
                        <ListItem key={m.memberId}>
                          <ListItemAvatar>
                            <Avatar
                              className={classes.memberAvatar}
                              dir={isRTL ? "rtl" : "ltr"}
                            >
                              {getInitials(m.name)}
                            </Avatar>
                          </ListItemAvatar>
                          <ListItemText
                            primary={
                              <span className={classes.memberName}>
                                {m.name}
                              </span>
                            }
                          />
                        </ListItem>
                      ))}
                      {students.length === 0 && (
                        <Typography
                          variant="body2"
                          className={classes.emptyNote}
                        >
                          {t("pages.classes.my.noStudents") || "No students."}
                        </Typography>
                      )}
                    </List>
                  </Box>
                </Box>
              )}

              {isFetching && <div className={classes.updatingLine} />}
            </>
          )}
        </Paper>
      </Box>
    </Box>
  );
};
