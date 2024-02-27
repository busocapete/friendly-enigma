import {
  createEntityAdapter,
  createSlice,
  createAsyncThunk,
} from "@reduxjs/toolkit";

import * as remoteClient from "../../dataServices/remoteClient";

const jobSearchAdapter = createEntityAdapter({
  selectId: (entity) => entity.document.id,
});

const initialState = jobSearchAdapter.getInitialState({
  status: "idle",
  loadMoreStatus: "idle",
  updateStatus: "idle",
  searching: false,
  currentPage: 1,
  nextPage: 2,
  totalCount: 0,
  firstFetchComplete: false,
  error: null,
  facets: [],
  selectedCheckboxes: [],
  filter: "",
  searchDto: {
    search: "",
    currentPage: 1,
    since: 0,
    industries: [],
    firmTypes: [],
    locations: [],
    sectors: [],
  },
});

export const searchJobs = createAsyncThunk(
  "search/jobs",
  async ({ searchDto, filter }) => {
    return await remoteClient.post(
      `/api/search/jobs?query=${filter}`,
      searchDto
    );
  }
);

export const updateSearch = createAsyncThunk(
  "/search/updateSearch",
  async ({ searchDto, filter }) => {
    return await remoteClient.post(
      `/api/search/jobs?query=${filter}`,
      searchDto
    );
  }
);

const jobSearchSlice = createSlice({
  name: "jobSearchResults",
  initialState,
  reducers: {
    updateFilter: (state, action) => {
      state.filter = action.payload;
    },
    toggleSavedItemOnSearch: (state, action) => {
      const itemId = action.payload;
      const existingItem = state.entities[itemId];
      if (existingItem) {
        existingItem.document.saved = !existingItem.document.saved;
      }
    },
    updateIndustries: (state, action) =>
      updateArrayInState(state, action, "industries"),
    updateLocations: (state, action) =>
      updateArrayInState(state, action, "locations"),
    updateSectors: (state, action) =>
      updateArrayInState(state, action, "sectors"),
    updateFirmTypes: (state, action) =>
      updateArrayInState(state, action, "firmTypes"),
    setSelectedCheckboxes: (state, action) => {
      const { value, filterType } = action.payload;
      const selectedCheckbox = `${filterType}_${value}`;
      if (state.selectedCheckboxes.includes(selectedCheckbox)) {
        // If already selected, remove it
        state.selectedCheckboxes = state.selectedCheckboxes.filter(
          (item) => item !== selectedCheckbox
        );
      } else {
        // If not selected, add it
        state.selectedCheckboxes.push(selectedCheckbox);
      }
    },
    setSince: (state, action) => {
      state.searchDto.since = action.payload;
    },
    setSearchDtoSince: (state, action) => {
      state.searchDto.since = action.payload;
    },
    setSearchDtoQuery: (state, action) => {
      state.searchDto.search = action.payload;
    },
    setSearchFilter: (state, action) => {
      state.filter = action.payload;
    },
    clearFilters: (state, action) => {
      state.selectedCheckboxes = [];
      state.searchDto = initialState.searchDto;
    },
    clearQuery: (state) => {
      state.filter = "";
    },
  },
  extraReducers(builder) {
    builder
      .addCase(searchJobs.pending, (state) => {
        state.status = "loading";
      })
      .addCase(searchJobs.fulfilled, (state, action) => {
        state.status = "succeeded";
        state.updateStatus = "succeeded";
        state.currentPage = action.payload.currentPage;
        jobSearchAdapter.setAll(state, action.payload.results);
        state.nextPage = state.currentPage++;
        state.totalCount = action.payload.count;
        state.firstFetchComplete = true;
        state.facets = action.payload.facets;
      })
      .addCase(searchJobs.rejected, (state, action) => {
        state.status = "failed";
        state.error = action.error.message;
      })
      .addCase(updateSearch.pending, (state) => {
        state.updateStatus = "loading";
      })
      .addCase(updateSearch.fulfilled, (state, action) => {
        state.updateStatus = "succeeded";
        state.currentPage = action.payload.currentPage;
        jobSearchAdapter.setAll(state, action.payload.results);
        state.nextPage = state.currentPage++;
        state.totalCount = action.payload.count;
        state.firstFetchComplete = true;
        state.facets = action.payload.facets;
      });
  },
});

export const {
  updateFilter,
  toggleSavedItemOnSearch,
  setSearchDtoQuery,
  updateIndustries,
  updateSectors,
  updateFirmTypes,
  updateLocations,
  setSelectedCheckboxes,
  setSearchDtoSince,
  setSearchFilter,
  clearQuery,
  clearFilters,
} = jobSearchSlice.actions;

export default jobSearchSlice.reducer;

export const {
  selectAll: selectAllJobs,
  selectById: selectJobById,
  selectIds: selectJobIds,
} = jobSearchAdapter.getSelectors((state) => state.jobSearchResults);

const updateArrayInState = (state, action, arrayName) => {
  const updatedArray = [...state.searchDto[arrayName]];
  const index = updatedArray.indexOf(action.payload);

  if (index !== -1) {
    // Remove the item if it exists
    updatedArray.splice(index, 1);
  } else {
    // Add the item if it doesn't exist
    updatedArray.push(action.payload);
  }

  return {
    ...state,
    searchDto: {
      ...state.searchDto,
      [arrayName]: updatedArray,
    },
  };
};
