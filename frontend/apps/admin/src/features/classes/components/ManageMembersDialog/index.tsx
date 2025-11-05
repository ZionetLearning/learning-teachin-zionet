import { useEffect, useMemo, useState } from "react";
import {
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  IconButton,
  InputAdornment,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
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
import { type User, type AppRoleType, useAuth } from "@app-providers";
import { RoleChip } from "@ui-components";

type Props = {
  open: boolean;
  classId: string;
  className: string;
  onClose: () => void;
};

type StudentTeacherRole = Exclude<AppRoleType, "admin">;

const getFullName = (u: User) => `${u.firstName} ${u.lastName}`.trim();

export const ManageMembersDialog = ({
  open,
  classId,
  className,
  onClose,
}: Props) => {
  const theme = useTheme();
  const { user } = useAuth();

  // Fetch class ONLY when dialog is open
  const { data: classData } = useGetClass(classId, { enabled: open });
  const { data: allUsers } = useGetAllUsers();

  const { mutate: addMembers, isPending: adding } = useAddClassMembers();
  const { mutate: removeMembers, isPending: removing } =
    useRemoveClassMembers();

  // ---- filters & search ----
  const [roleFilter, setRoleFilter] = useState<StudentTeacherRole | "All">(
    "All",
  );
  const [query, setQuery] = useState("");

  // ---- multi-selections ----
  const [selectedCandidateIds, setSelectedCandidateIds] = useState<Set<string>>(
    new Set(),
  );
  const [selectedMemberIds, setSelectedMemberIds] = useState<Set<string>>(
    new Set(),
  );

  // Clear selections when dialog opens or class changes
  useEffect(() => {
    if (!open) return;
    setSelectedCandidateIds(new Set());
    setSelectedMemberIds(new Set());
  }, [open, classId]);

  // Candidates = only students/teachers (no admins)
  const candidateUsers = useMemo(
    () =>
      (allUsers ?? []).filter(
        (u) => u.role === "student" || u.role === "teacher",
      ),
    [allUsers],
  );

  // Fast lookup set of current member IDs
  const memberIdSet = useMemo(
    () => new Set((classData?.members ?? []).map((m) => m.memberId)),
    [classData?.members],
  );

  // Role counts
  const studentsCount = candidateUsers.filter(
    (u) => u.role === "student",
  ).length;
  const teachersCount = candidateUsers.filter(
    (u) => u.role === "teacher",
  ).length;

  // Left list: filtered users
  const filteredUsers = useMemo(() => {
    const base = candidateUsers.filter((u) =>
      roleFilter === "All" ? true : u.role === roleFilter,
    );
    if (!query) return base;
    const q = query.toLowerCase();
    return base.filter(
      (u) =>
        getFullName(u).toLowerCase().includes(q) ||
        u.email.toLowerCase().includes(q),
    );
  }, [candidateUsers, roleFilter, query]);

  // Right list: current members
  const visibleMembers = useMemo(
    () => classData?.members ?? [],
    [classData?.members],
  );

  // ---- selection helpers ----
  const toggleCandidate = (userId: string) =>
    setSelectedCandidateIds((prev) => {
      const next = new Set(prev);
      if (next.has(userId)) next.delete(userId);
      else next.add(userId);
      return next;
    });

  const toggleMember = (memberId: string) =>
    setSelectedMemberIds((prev) => {
      const next = new Set(prev);
      if (next.has(memberId)) next.delete(memberId);
      else next.add(memberId);
      return next;
    });

  const selectAllCandidates = () => {
    const ids = filteredUsers
      .filter((u) => !memberIdSet.has(u.userId))
      .map((u) => u.userId);
    setSelectedCandidateIds(new Set(ids));
  };
  const clearAllCandidates = () => setSelectedCandidateIds(new Set());

  const selectAllMembers = () => {
    const ids = visibleMembers.map((m) => m.memberId);
    setSelectedMemberIds(new Set(ids));
  };
  const clearAllMembers = () => setSelectedMemberIds(new Set());

  // ---- actions ----
  const handleAdd = (ids: string[]) => {
    if (!ids.length) return;
    addMembers({ classId, userIds: ids, addedBy: user?.userId || "" });
  };

  const handleRemove = (ids: string[]) => {
    if (!ids.length) return;
    removeMembers({ classId, userIds: ids });
  };

  const handleBatchAdd = () => {
    const ids = Array.from(selectedCandidateIds).filter(
      (id) => !memberIdSet.has(id),
    );
    if (!ids.length) return;
    handleAdd(ids);
    setSelectedCandidateIds(new Set()); // clear selection
  };

  const handleBatchRemove = () => {
    const ids = Array.from(selectedMemberIds);
    if (!ids.length) return;
    handleRemove(ids);
    setSelectedMemberIds(new Set()); // clear selection
  };

  // helper style to highlight selected rows (optional)
  const selectedBg = theme.vars?.palette?.primary?.mainChannel
    ? `rgba(var(--mui-palette-primary-mainChannel)/0.08)`
    : theme.palette.action.selected;

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="md">
      <DialogTitle>Manage Members — {className}</DialogTitle>
      <DialogContent>
        <Stack direction={{ xs: "column", md: "row" }} gap={2} sx={{ mt: 1 }}>
          {/* LEFT: filters & candidates */}
          <Box sx={{ flex: 1, minWidth: 320 }}>
            <Stack direction="row" gap={1} alignItems="center" sx={{ mb: 1 }}>
              <Select
                size="small"
                value={roleFilter}
                onChange={(e) =>
                  setRoleFilter(e.target.value as StudentTeacherRole | "All")
                }
                sx={{ minWidth: 160 }}
              >
                <MenuItem value="All">All roles</MenuItem>
                <MenuItem value="student">Students ({studentsCount})</MenuItem>
                <MenuItem value="teacher">Teachers ({teachersCount})</MenuItem>
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

            {/* Candidate toolbar */}
            <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
              <Button size="small" onClick={selectAllCandidates}>
                Select all
              </Button>
              <Button size="small" onClick={clearAllCandidates}>
                Clear
              </Button>
              <Button
                size="small"
                variant="contained"
                onClick={handleBatchAdd}
                disabled={
                  adding ||
                  Array.from(selectedCandidateIds).filter(
                    (id) => !memberIdSet.has(id),
                  ).length === 0
                }
              >
                Add selected (
                {
                  Array.from(selectedCandidateIds).filter(
                    (id) => !memberIdSet.has(id),
                  ).length
                }
                )
              </Button>
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
                const isMember = memberIdSet.has(u.userId);
                const checked = selectedCandidateIds.has(u.userId);

                const labelId = `candidate-${u.userId}`;

                return (
                  <ListItem
                    key={u.userId}
                    disablePadding
                    secondaryAction={
                      <Tooltip
                        title={isMember ? "Already in class" : "Add to class"}
                      >
                        <span>
                          <IconButton
                            edge="end"
                            onClick={(e) => {
                              e.stopPropagation();
                              if (!isMember) handleAdd([u.userId]);
                            }}
                            disabled={adding || isMember}
                          >
                            <AddIcon />
                          </IconButton>
                        </span>
                      </Tooltip>
                    }
                    sx={{
                      opacity: isMember ? 0.55 : 1,
                    }}
                  >
                    <ListItemButton
                      dense
                      onClick={() => !isMember && toggleCandidate(u.userId)}
                      sx={{
                        cursor: isMember ? "not-allowed" : "pointer",
                        bgcolor: checked ? selectedBg : undefined,
                        pr: 8, // space for secondaryAction
                      }}
                    >
                      <ListItemIcon sx={{ minWidth: 40 }}>
                        <Checkbox
                          edge="start"
                          checked={checked}
                          disabled={isMember}
                          tabIndex={-1}
                          disableRipple
                          onClick={(e) => e.stopPropagation()}
                          onChange={() =>
                            !isMember && toggleCandidate(u.userId)
                          }
                          inputProps={{ "aria-labelledby": labelId }}
                        />
                      </ListItemIcon>
                      <ListItemText
                        id={labelId}
                        primary={
                          <Stack direction="row" gap={1} alignItems="center">
                            <Typography>{getFullName(u)}</Typography>
                            <RoleChip
                              role={u.role}
                              data-testid="users-role-badge"
                            />
                          </Stack>
                        }
                        secondary={u.email}
                      />
                    </ListItemButton>
                  </ListItem>
                );
              })}

              {filteredUsers.length === 0 && (
                <Box p={2}>
                  <Typography variant="body2" color="text.secondary">
                    No users match the current filters.
                  </Typography>
                </Box>
              )}
            </List>
          </Box>

          <Divider flexItem orientation="vertical" />

          {/* RIGHT: current members */}
          <Box sx={{ flex: 1, minWidth: 320 }}>
            <Typography variant="subtitle1" sx={{ mb: 1 }}>
              Current Members ({visibleMembers.length})
            </Typography>

            {/* Members toolbar */}
            <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
              <Button size="small" onClick={selectAllMembers}>
                Select all
              </Button>
              <Button size="small" onClick={clearAllMembers}>
                Clear
              </Button>
              <Button
                size="small"
                variant="contained"
                color="error"
                onClick={handleBatchRemove}
                disabled={removing || selectedMemberIds.size === 0}
              >
                Remove selected ({selectedMemberIds.size})
              </Button>
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
              {visibleMembers.map((m) => {
                const checked = selectedMemberIds.has(m.memberId);
                const labelId = `member-${m.memberId}`;

                return (
                  <ListItem
                    key={m.memberId}
                    disablePadding
                    secondaryAction={
                      <Tooltip title="Remove from class">
                        <span>
                          <IconButton
                            edge="end"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleRemove([m.memberId]);
                            }}
                            disabled={removing}
                          >
                            <RemoveIcon />
                          </IconButton>
                        </span>
                      </Tooltip>
                    }
                  >
                    <ListItemButton
                      dense
                      onClick={() => toggleMember(m.memberId)}
                      sx={{ pr: 8, bgcolor: checked ? selectedBg : undefined }}
                    >
                      <ListItemIcon sx={{ minWidth: 40 }}>
                        <Checkbox
                          edge="start"
                          checked={checked}
                          tabIndex={-1}
                          disableRipple
                          onClick={(e) => e.stopPropagation()}
                          onChange={() => toggleMember(m.memberId)}
                          inputProps={{ "aria-labelledby": labelId }}
                        />
                      </ListItemIcon>
                      <ListItemText
                        id={labelId}
                        primary={
                          <Stack direction="row" gap={1} alignItems="center">
                            <Typography>{m.name}</Typography>
                            <RoleChip
                              role={m.role === 1 ? "teacher" : "student"}
                              data-testid="users-role-badge"
                            />
                          </Stack>
                        }
                      />
                    </ListItemButton>
                  </ListItem>
                );
              })}

              {visibleMembers.length === 0 && (
                <Box p={2}>
                  <Typography variant="body2" color="text.secondary">
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
