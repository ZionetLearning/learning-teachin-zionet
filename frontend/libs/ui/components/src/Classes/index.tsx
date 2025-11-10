import { useMemo, useState } from "react";
import {
  Box,
  Paper,
  List,
  ListItemButton,
  ListItemText,
  Typography,
  ListItem,
  Divider,
  Grid,
} from "@mui/material";

import { useMyClasses, type Member, type ClassItem } from "@api";

export const Classes = () => {
  const { data: classes, isLoading, isError, isFetching } = useMyClasses();
  const [selectedClassId, setSelectedClassId] = useState<string | undefined>();

  // pick the selected class from the list
  const selectedClass: ClassItem | undefined = useMemo(() => {
    if (!classes || !selectedClassId) return undefined;
    return classes.find((c) => c.classId === selectedClassId);
  }, [classes, selectedClassId]);

  // split by role (0=Student, 1=Teacher per your payload)
  const { teachers, students } = useMemo(() => {
    const none = { teachers: [] as Member[], students: [] as Member[] };
    if (!selectedClass?.members) return none;
    return {
      teachers: selectedClass.members.filter((m) => m.role === 1),
      students: selectedClass.members.filter((m) => m.role === 0),
    };
  }, [selectedClass]);

  if (isLoading) return <Typography>Loading classes…</Typography>;
  if (isError)
    return <Typography color="error">Failed to load classes.</Typography>;
  if (!classes || classes.length === 0)
    return <Typography>No classes yet.</Typography>;

  return (
    <Box display="grid" gridTemplateColumns="320px 1fr" gap={2}>
      {/* Left: classes list */}
      <Paper>
        <List dense disablePadding>
          {classes.map((c) => (
            <ListItemButton
              key={c.classId}
              selected={c.classId === selectedClassId}
              onClick={() => setSelectedClassId(c.classId)}
            >
              <ListItemText
                primary={c.name}
                secondary={
                  isFetching && c.classId === selectedClassId
                    ? "Updating…"
                    : undefined
                }
              />
            </ListItemButton>
          ))}
        </List>
      </Paper>

      {/* Right: members split into two columns */}
      <Paper style={{ padding: 16 }}>
        {!selectedClass ? (
          <Typography>Select a class…</Typography>
        ) : selectedClass.members.length === 0 ? (
          <Typography>No members.</Typography>
        ) : (
          <>
            <Typography variant="h6" gutterBottom>
              {selectedClass.name}
            </Typography>
            <Divider sx={{ mb: 2 }} />

            <Grid container spacing={2}>
              <Grid size={{ xs: 12, md: 6 }}>
                <Typography variant="subtitle1" gutterBottom>
                  Teachers ({teachers.length})
                </Typography>
                <List dense>
                  {teachers.map((m) => (
                    <ListItem key={m.memberId}>
                      <ListItemText primary={m.name} />
                    </ListItem>
                  ))}
                  {teachers.length === 0 && (
                    <Typography variant="body2" color="text.secondary">
                      No teachers.
                    </Typography>
                  )}
                </List>
              </Grid>

              <Grid size={{ xs: 12, md: 6 }}>
                <Typography variant="subtitle1" gutterBottom>
                  Students ({students.length})
                </Typography>
                <List dense>
                  {students.map((m) => (
                    <ListItem key={m.memberId}>
                      <ListItemText primary={m.name} />
                    </ListItem>
                  ))}
                  {students.length === 0 && (
                    <Typography variant="body2" color="text.secondary">
                      No students.
                    </Typography>
                  )}
                </List>
              </Grid>
            </Grid>
          </>
        )}
      </Paper>
    </Box>
  );
};
