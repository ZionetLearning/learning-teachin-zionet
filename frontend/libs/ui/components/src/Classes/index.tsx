import { useState } from "react";
import {
  Box,
  Paper,
  List,
  ListItemButton,
  ListItemText,
  Typography,
} from "@mui/material";
import { useMyClasses, useClassMembers, type RoleMode } from "@api";

type Props = {
  mode: RoleMode; // "Teacher" | "Student"
  onSelectClass?: (classId: string) => void;
};

export const Classes = ({ mode, onSelectClass }: Props) => {
  const { data: classes, isLoading, isError } = useMyClasses(mode);
  const [selectedClassId, setSelectedClassId] = useState<string | undefined>(
    undefined,
  );

  const { data: members, isLoading: membersLoading } = useClassMembers(
    selectedClassId,
    mode,
    { enabled: Boolean(selectedClassId) },
  );

  if (isLoading) return <Typography>Loading classes…</Typography>;
  if (isError)
    return <Typography color="error">Failed to load classes.</Typography>;
  if (!classes || classes.length === 0)
    return <Typography>No classes yet.</Typography>;

  return (
    <Box display="grid" gridTemplateColumns="320px 1fr" gap={2}>
      <Paper>
        <List>
          {classes.map((c) => (
            <ListItemButton
              key={c.classId}
              selected={c.classId === selectedClassId}
              onClick={() => {
                setSelectedClassId(c.classId);
                onSelectClass?.(c.classId);
              }}
            >
              <ListItemText primary={c.name} />
            </ListItemButton>
          ))}
        </List>
      </Paper>

      <Paper style={{ padding: 16 }}>
        {!selectedClassId ? (
          <Typography>Select a class…</Typography>
        ) : membersLoading ? (
          <Typography>Loading members…</Typography>
        ) : !members || members.length === 0 ? (
          <Typography>No members.</Typography>
        ) : (
          <>
            <Typography variant="h6" gutterBottom>
              {mode === "Teacher" ? "Students" : "Teachers"}
            </Typography>
            <List>
              {members.map((m) => (
                <ListItemText key={m.memberId} primary={m.name} />
              ))}
            </List>
          </>
        )}
      </Paper>
    </Box>
  );
};
