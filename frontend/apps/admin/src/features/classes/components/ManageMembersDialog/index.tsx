import { useEffect, useMemo, useState } from "react";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Stack,
} from "@mui/material";

import {
  useAddClassMembers,
  useGetClass,
  useRemoveClassMembers,
  useGetAllUsers,
} from "@admin/api";
import { type User, type AppRoleType, useAuth } from "@app-providers";

import { CandidateListPanel, ClassMembersListPanel } from "./elements";

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
  const { user } = useAuth();

  // fetch only when dialog open
  const { data: classData } = useGetClass(classId, { enabled: open });
  const { data: allUsers } = useGetAllUsers();

  const { mutate: addMembers, isPending: adding } = useAddClassMembers();
  const { mutate: removeMembers, isPending: removing } =
    useRemoveClassMembers();

  // filters
  const [roleFilter, setRoleFilter] = useState<StudentTeacherRole | "All">(
    "All",
  );
  const [query, setQuery] = useState("");

  // selections
  const [selectedCandidateIds, setSelectedCandidateIds] = useState<Set<string>>(
    new Set(),
  );
  const [selectedMemberIds, setSelectedMemberIds] = useState<Set<string>>(
    new Set(),
  );

  // reset selections when open/class changes
  useEffect(() => {
    if (!open) return;
    setSelectedCandidateIds(new Set());
    setSelectedMemberIds(new Set());
  }, [open, classId]);

  // data shaping
  const candidateUsers = useMemo(
    () =>
      (allUsers ?? []).filter(
        (u) => u.role === "student" || u.role === "teacher",
      ),
    [allUsers],
  );

  const memberIdSet = useMemo(
    () => new Set((classData?.members ?? []).map((m) => m.memberId)),
    [classData?.members],
  );

  const studentsCount = candidateUsers.filter(
    (u) => u.role === "student",
  ).length;
  const teachersCount = candidateUsers.filter(
    (u) => u.role === "teacher",
  ).length;

  const filteredUsers = useMemo(() => {
    const base =
      roleFilter === "All"
        ? candidateUsers
        : candidateUsers.filter((u) => u.role === roleFilter);
    if (!query) return base;
    const q = query.toLowerCase();
    return base.filter(
      (u) =>
        getFullName(u).toLowerCase().includes(q) ||
        u.email.toLowerCase().includes(q),
    );
  }, [candidateUsers, roleFilter, query]);

  const visibleMembers = useMemo(
    () => classData?.members ?? [],
    [classData?.members],
  );

  // toggle handlers
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

  // select/clear all
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

  // actions
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
    setSelectedCandidateIds(new Set());
  };
  const handleBatchRemove = () => {
    const ids = Array.from(selectedMemberIds);
    if (!ids.length) return;
    handleRemove(ids);
    setSelectedMemberIds(new Set());
  };

  // count of selected addable candidates (not already in class)
  const addableCount = useMemo(
    () =>
      Array.from(selectedCandidateIds).filter((id) => !memberIdSet.has(id))
        .length,
    [selectedCandidateIds, memberIdSet],
  );

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="md">
      <DialogTitle>Manage Members — {className}</DialogTitle>
      <DialogContent>
        <Stack direction={{ xs: "column", md: "row" }} gap={2} sx={{ mt: 1 }}>
          <CandidateListPanel
            users={filteredUsers}
            memberIdSet={memberIdSet}
            selectedIds={selectedCandidateIds}
            onToggle={toggleCandidate}
            onSelectAll={selectAllCandidates}
            onClearAll={clearAllCandidates}
            onAddSingle={(id) => handleAdd([id])}
            onBatchAdd={handleBatchAdd}
            pendingAdd={adding}
            addableCount={addableCount}
            roleFilter={roleFilter}
            setRoleFilter={setRoleFilter}
            query={query}
            setQuery={setQuery}
            studentsCount={studentsCount}
            teachersCount={teachersCount}
          />

          <Divider flexItem orientation="vertical" />

          <ClassMembersListPanel
            members={visibleMembers}
            selectedIds={selectedMemberIds}
            onToggle={toggleMember}
            onSelectAll={selectAllMembers}
            onClearAll={clearAllMembers}
            onRemoveSingle={(id) => handleRemove([id])}
            onBatchRemove={handleBatchRemove}
            pendingRemove={removing}
          />
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
