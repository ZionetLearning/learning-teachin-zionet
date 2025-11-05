import {
  Box,
  Button,
  Checkbox,
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

import { type User, type AppRoleType } from "@app-providers";
import { RoleChip } from "@ui-components";

type StudentTeacherRole = Exclude<AppRoleType, "admin">;

type Props = {
  users: User[]; // only students/teachers (already filtered for "no admin")
  memberIdSet: Set<string>;
  selectedIds: Set<string>;
  onToggle: (userId: string) => void;

  // toolbar
  onSelectAll: () => void;
  onClearAll: () => void;

  // actions
  onAddSingle: (userId: string) => void;
  onBatchAdd: () => void;
  pendingAdd: boolean;

  // UX numbers
  addableCount: number;

  // counts for header filters
  roleFilter: StudentTeacherRole | "All";
  setRoleFilter: (r: StudentTeacherRole | "All") => void;
  query: string;
  setQuery: (s: string) => void;
  studentsCount: number;
  teachersCount: number;
};

const getFullName = (u: User) => `${u.firstName} ${u.lastName}`.trim();

export const CandidateListPanel = ({
  users,
  memberIdSet,
  selectedIds,
  onToggle,
  onSelectAll,
  onClearAll,
  onAddSingle,
  onBatchAdd,
  pendingAdd,
  addableCount,
  roleFilter,
  setRoleFilter,
  query,
  setQuery,
  studentsCount,
  teachersCount,
}: Props) => {
  const theme = useTheme();

  const selectedBg = theme.vars?.palette?.primary?.mainChannel
    ? `rgba(var(--mui-palette-primary-mainChannel)/0.08)`
    : theme.palette.action.selected;

  return (
    <Box sx={{ flex: 1, minWidth: 320 }}>
      {/* Filters */}
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

      {/* Toolbar */}
      <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
        <Button size="small" onClick={onSelectAll}>
          Select all
        </Button>
        <Button size="small" onClick={onClearAll}>
          Clear
        </Button>
        <Button
          size="small"
          variant="contained"
          onClick={onBatchAdd}
          disabled={pendingAdd || addableCount === 0}
        >
          Add selected ({addableCount})
        </Button>
      </Stack>

      {/* List */}
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
        {users.map((u) => {
          const isMember = memberIdSet.has(u.userId);
          const checked = selectedIds.has(u.userId);
          const labelId = `candidate-${u.userId}`;

          return (
            <ListItem
              key={u.userId}
              disablePadding
              secondaryAction={
                <Tooltip title={isMember ? "Already in class" : "Add to class"}>
                  <span>
                    <IconButton
                      edge="end"
                      onClick={(e) => {
                        e.stopPropagation();
                        if (!isMember) onAddSingle(u.userId);
                      }}
                      disabled={pendingAdd || isMember}
                    >
                      <AddIcon />
                    </IconButton>
                  </span>
                </Tooltip>
              }
              sx={{ opacity: isMember ? 0.55 : 1 }}
            >
              <ListItemButton
                dense
                onClick={() => !isMember && onToggle(u.userId)}
                sx={{
                  cursor: isMember ? "not-allowed" : "pointer",
                  bgcolor: checked ? selectedBg : undefined,
                  pr: 8, // space for secondary action
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
                    onChange={() => !isMember && onToggle(u.userId)}
                    inputProps={{ "aria-labelledby": labelId }}
                  />
                </ListItemIcon>

                <ListItemText
                  id={labelId}
                  primary={
                    <Stack direction="row" gap={1} alignItems="center">
                      <Typography>{getFullName(u)}</Typography>
                      <RoleChip role={u.role} data-testid="users-role-badge" />
                    </Stack>
                  }
                  secondary={u.email}
                />
              </ListItemButton>
            </ListItem>
          );
        })}

        {users.length === 0 && (
          <Box p={2}>
            <Typography variant="body2" color="text.secondary">
              No users match the current filters.
            </Typography>
          </Box>
        )}
      </List>
    </Box>
  );
};
