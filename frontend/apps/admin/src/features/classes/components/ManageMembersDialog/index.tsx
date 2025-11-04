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
import { type User, type AppRoleType } from "@app-providers";

type Props = {
  open: boolean;
  classId: string;
  className: string;
  onClose: () => void;
};

const getFullName = (u: User) => `${u.firstName} ${u.lastName}`.trim();

export const ManageMembersDialog = ({
  open,
  classId,
  className,
  onClose,
}: Props) => {
  const theme = useTheme();

  const { data: classData } = useGetClass(classId);
  const { data: allUsers } = useGetAllUsers();
  const { mutate: addMembers, isPending: adding } = useAddClassMembers();
  const { mutate: removeMembers, isPending: removing } =
    useRemoveClassMembers();

  const [roleFilter, setRoleFilter] = useState<AppRoleType | "All">("All");
  const [query, setQuery] = useState("");

  // Members set
  const memberIds = useMemo(
    () => new Set(classData?.members ?? []),
    [classData?.members],
  );

  // Role counts (lowercase roles per AppRoleType)
  const studentsCount = (allUsers ?? []).filter(
    (u) => u.role === "student",
  ).length;
  const teachersCount = (allUsers ?? []).filter(
    (u) => u.role === "teacher",
  ).length;

  // Filter users by role + query
  const filteredUsers = useMemo(() => {
    const list = (allUsers ?? []) as User[];
    return list
      .filter((u) => (roleFilter === "All" ? true : u.role === roleFilter))
      .filter((u) => {
        if (!query) return true;
        const q = query.toLowerCase();
        return (
          getFullName(u).toLowerCase().includes(q) ||
          u.email.toLowerCase().includes(q)
        );
      });
  }, [allUsers, roleFilter, query]);

  const handleAdd = (ids: string[]) => {
    if (!ids.length) return;
    addMembers(
      { classId, userIds: ids, addedBy: "admin" /* TODO: real admin id */ },
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
                onChange={(e) =>
                  setRoleFilter(e.target.value as AppRoleType | "All")
                }
                sx={{ minWidth: 160 }}
              >
                <MenuItem value="All">All roles</MenuItem>
                <MenuItem value="student">Students ({studentsCount})</MenuItem>
                <MenuItem value="teacher">Teachers ({teachersCount})</MenuItem>
                <MenuItem value="admin">Admins</MenuItem>
              </Select>

              <TextField
                size="small"
                placeholder="Search by name / email"
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
                border: `${theme.palette.divider} 1px solid`,
                bgcolor:
                  theme.palette.mode === "dark"
                    ? "rgba(255,255,255,0.03)"
                    : "background.paper",
                maxHeight: 360,
                overflow: "auto",
              }}
            >
              {filteredUsers.map((u) => {
                const isMember = memberIds.has(u.userId);
                return (
                  <ListItem
                    key={u.userId}
                    secondaryAction={
                      isMember ? (
                        <Tooltip title="Remove from class">
                          <span>
                            <IconButton
                              edge="end"
                              onClick={() => handleRemove([u.userId])}
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
                              onClick={() => handleAdd([u.userId])}
                              disabled={adding}
                            >
                              <AddIcon />
                            </IconButton>
                          </span>
                        </Tooltip>
                      )
                    }
                  >
                    <ListItemText
                      primary={
                        <Stack direction="row" gap={1} alignItems="center">
                          <Typography>{getFullName(u)}</Typography>
                          <Chip
                            size="small"
                            label={u.role}
                            variant="outlined"
                          />
                        </Stack>
                      }
                      secondary={u.email}
                    />
                  </ListItem>
                );
              })}

              {filteredUsers.length === 0 && (
                <Box p={2}>
                  <Typography variant="body2">
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
                border: `${theme.palette.divider} 1px solid`,
                bgcolor:
                  theme.palette.mode === "dark"
                    ? "rgba(255,255,255,0.03)"
                    : "background.paper",
                maxHeight: 360,
                overflow: "auto",
              }}
            >
              {(allUsers ?? [])
                .filter((u) => memberIds.has(u.userId))
                .map((u) => (
                  <ListItem
                    key={u.userId}
                    secondaryAction={
                      <Tooltip title="Remove from class">
                        <span>
                          <IconButton
                            edge="end"
                            onClick={() => handleRemove([u.userId])}
                            disabled={removing}
                          >
                            <RemoveIcon />
                          </IconButton>
                        </span>
                      </Tooltip>
                    }
                  >
                    <ListItemText
                      primary={
                        <Stack direction="row" gap={1} alignItems="center">
                          <Typography>{getFullName(u)}</Typography>
                          <Chip
                            size="small"
                            label={u.role}
                            variant="outlined"
                          />
                        </Stack>
                      }
                      secondary={u.userId}
                    />
                  </ListItem>
                ))}

              {memberIds.size === 0 && (
                <Box p={2}>
                  <Typography variant="body2">
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
