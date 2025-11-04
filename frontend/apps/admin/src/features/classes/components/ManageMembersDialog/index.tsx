import { useMemo, useState } from "react";
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  IconButton,
  InputAdornment,
  List,
  ListItem,
  ListItemSecondaryAction,
  ListItemText,
  MenuItem,
  Select,
  Stack,
  TextField,
  Tooltip,
  Typography,
  useTheme,
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import AddIcon from "@mui/icons-material/PersonAddAlt";
import RemoveIcon from "@mui/icons-material/PersonRemove";
import {
  useAddClassMembers,
  useGetClass,
  useRemoveClassMembers,
  useGetAllUsers,
} from "@admin/api";

// Minimal user shape — align to your real type
type Role = "Student" | "Teacher" | "Admin";
type User = {
  id: string;
  fullName: string;
  role: Role;
};

type Props = {
  open: boolean;
  classId: string;
  className: string;
  onClose: () => void;
};

export const ManageMembersDialog = ({
  open,
  classId,
  className,
  onClose,
}: Props) => {
  const theme = useTheme();
  const { data: classData } = useGetClass(classId, { enabled: open });
  const { data: allUsers } = useGetAllUsers();
  const { mutate: addMembers, isLoading: adding } = useAddClassMembers();
  const { mutate: removeMembers, isLoading: removing } =
    useRemoveClassMembers();

  const [roleFilter, setRoleFilter] = useState<Role | "All">("All");
  const [query, setQuery] = useState("");

  const memberIds = useMemo(
    () => new Set(classData?.members ?? []),
    [classData?.members],
  );

  const filteredUsers = useMemo(() => {
    const list = (allUsers ?? []) as User[];
    return list
      .filter((u) => (roleFilter === "All" ? true : u.role === roleFilter))
      .filter((u) =>
        query
          ? u.fullName.toLowerCase().includes(query.toLowerCase()) ||
            u.id.toLowerCase().includes(query.toLowerCase())
          : true,
      );
  }, [allUsers, roleFilter, query]);

  const studentsCount = (allUsers ?? []).filter(
    (u: any) => u.role === "Student",
  ).length;
  const teachersCount = (allUsers ?? []).filter(
    (u: any) => u.role === "Teacher",
  ).length;

  const handleAdd = (ids: string[]) => {
    if (!ids.length) return;
    addMembers(
      { classId, userIds: ids, addedBy: "admin" /* set to real admin id */ },
      { onSuccess: () => {} },
    );
  };

  const handleRemove = (ids: string[]) => {
    if (!ids.length) return;
    removeMembers({ classId, userIds: ids }, { onSuccess: () => {} });
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="md">
      <DialogTitle>Manage Members — {className}</DialogTitle>
      <DialogContent>
        <Stack direction={{ xs: "column", md: "row" }} gap={2} sx={{ mt: 1 }}>
          {/* Left panel: filters & users */}
          <Box sx={{ flex: 1, minWidth: 320 }}>
            <Stack direction="row" gap={1} alignItems="center" sx={{ mb: 1 }}>
              <Select
                size="small"
                value={roleFilter}
                onChange={(e) => setRoleFilter(e.target.value as any)}
                sx={{ minWidth: 140 }}
              >
                <MenuItem value="All">All roles</MenuItem>
                <MenuItem value="Student">Students ({studentsCount})</MenuItem>
                <MenuItem value="Teacher">Teachers ({teachersCount})</MenuItem>
                <MenuItem value="Admin">Admins</MenuItem>
              </Select>
              <TextField
                size="small"
                placeholder="Search by name/id"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchIcon />
                    </InputAdornment>
                  ),
                }}
                fullWidth
              />
            </Stack>

            <List
              dense
              sx={{
                borderRadius: 2,
                border: theme.palette.divider + " 1px solid",
                bgcolor:
                  theme.palette.mode === "dark"
                    ? "rgba(255,255,255,0.03)"
                    : "background.paper",
                maxHeight: 360,
                overflow: "auto",
              }}
            >
              {filteredUsers.map((u) => {
                const isMember = memberIds.has(u.id);
                return (
                  <ListItem key={u.id}>
                    <ListItemText
                      primary={
                        <Stack direction="row" gap={1} alignItems="center">
                          <Typography>{u.fullName}</Typography>
                          <Chip
                            size="small"
                            label={u.role}
                            variant="outlined"
                          />
                        </Stack>
                      }
                      secondary={u.id}
                    />
                    <ListItemSecondaryAction>
                      {isMember ? (
                        <Tooltip title="Remove from class">
                          <span>
                            <IconButton
                              edge="end"
                              onClick={() => handleRemove([u.id])}
                              disabled={removing}
                            >
                              <RemoveIcon />
                            </IconButton>
                          </span>
                        </Tooltip>
                      ) : (
                        <Tooltip title="Add to class">
                          <span>
                            <IconButton
                              edge="end"
                              onClick={() => handleAdd([u.id])}
                              disabled={adding}
                            >
                              <AddIcon />
                            </IconButton>
                          </span>
                        </Tooltip>
                      )}
                    </ListItemSecondaryAction>
                  </ListItem>
                );
              })}
              {filteredUsers.length === 0 && (
                <Box p={2}>
                  <Typography variant="body2" opacity={0.7}>
                    No users match the current filters.
                  </Typography>
                </Box>
              )}
            </List>
          </Box>

          <Divider flexItem orientation="vertical" />

          {/* Right panel: current members */}
          <Box sx={{ flex: 1, minWidth: 320 }}>
            <Typography variant="subtitle1" sx={{ mb: 1 }}>
              Current Members ({memberIds.size})
            </Typography>
            <List
              dense
              sx={{
                borderRadius: 2,
                border: theme.palette.divider + " 1px solid",
                bgcolor:
                  theme.palette.mode === "dark"
                    ? "rgba(255,255,255,0.03)"
                    : "background.paper",
                maxHeight: 360,
                overflow: "auto",
              }}
            >
              {(allUsers ?? [])
                .filter((u: any) => memberIds.has(u.id))
                .map((u: User) => (
                  <ListItem key={u.id}>
                    <ListItemText
                      primary={
                        <Stack direction="row" gap={1} alignItems="center">
                          <Typography>{u.fullName}</Typography>
                          <Chip
                            size="small"
                            label={u.role}
                            variant="outlined"
                          />
                        </Stack>
                      }
                      secondary={u.id}
                    />
                    <ListItemSecondaryAction>
                      <Tooltip title="Remove from class">
                        <span>
                          <IconButton
                            edge="end"
                            onClick={() => handleRemove([u.id])}
                            disabled={removing}
                          >
                            <RemoveIcon />
                          </IconButton>
                        </span>
                      </Tooltip>
                    </ListItemSecondaryAction>
                  </ListItem>
                ))}
              {memberIds.size === 0 && (
                <Box p={2}>
                  <Typography variant="body2" opacity={0.7}>
                    This class has no members yet.
                  </Typography>
                </Box>
              )}
            </List>
          </Box>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="inherit">
          Close
        </Button>
      </DialogActions>
    </Dialog>
  );
};
