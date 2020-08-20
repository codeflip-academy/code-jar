<template lang="pug">
.card.mb-3(:class="{ 'bg-dark text-white': $darkMode }")
  .card-header.d-flex.justify-content-between.align-items-start
    .project-name
      i.fa.fa-table
      |
      | Random Codes

  .card-body
    //- Batch view
    .single-batch(v-if="batchSelected")
      button.mb-2.btn.btn-sm.btn-light(@click="GoBack()")
        i.fa.fa-angle-left
        |
        | Batch list
      code-table(:batchID="batchIDSelected")

    //- Search results view
    .search-results(v-else-if="isSearching")
      button.mb-2.mr-2.btn.btn-sm.btn-light(@click="GoBack()")
        i.fa.fa-angle-left
        |
        | Batch list
      span Search results for <em>{{ codeSearchVal }}</em>
      code-table(:search="codeSearchVal")

    //- Show batch list when batch isn't selected
    .batch-list(v-if="!batchSelected && !isSearching")
      form.form-inline.mb-3(v-on:submit.prevent="CreateBatch()")
        //- Batch Name
        input#batchName.form-control.form-control-sm.mr-sm-2.mb-2.mb-sm-0(
          v-model="batchName",
          type="text",
          placeholder="Batch name",
          maxlength="50",
          required
        )

        //- Batch Name
        input#promotionType.form-control.form-control-sm.mr-sm-2.mb-2.mb-sm-0(
          v-model="promotionType",
          type="text",
          placeholdecr="Promotion type",
          maxlength="50",
          required
        )

        //- Date Active
        input#dateActive.form-control.form-control-sm.mr-sm-2.mb-2.mb-sm-0(
          v-model="dateActive",
          type="text",
          onfocus="(this.type='date')",
          :min="dateActiveMin",
          placeholder="Start Date",
          required
        )

        //- Date Expires
        input#dateExpires.form-control.form-control-sm.mr-sm-2.mb-2.mb-sm-0(
          v-model="dateExpires",
          type="text",
          :min="dateExpiresMin",
          onfocus="(this.type='date')",
          placeholder="End Date"
        )

        //- Batch Size
        input#batchSize.form-control.form-control-sm.mr-sm-2.mb-2.mb-sm-0(
          v-model="batchSize",
          type="number",
          min="0",
          max="10000",
          placeholder="Number of codes"
        )

        //- Create Batch
        button.btn.btn-sm.btn-primary(type="submit") Create Batch

      //- Search codes form
      form.form-inline(v-on:submit.prevent="SearchCodes()")
        input#codeSearchVal.form-control.form-control-sm.mr-sm-2.mb-2.mb-sm-0(
          v-model="codeSearchVal",
          placeholder="Search codes",
          pattern=".{6,}",
          maxlength="6",
          required
        )

        button.btn.btn-sm.btn-primary(type="submit") Search codes

      //- Success or failure for batch creation
      div(v-if="formSubmitted")
        .alert.alert-danger.mt-3(v-if="batchError") Couldn't create batch! Try again.
        .alert.alert-success.mt-3(v-else) Batch was created successfully.

      //- Batch list
      .batch.card.m-0.mt-3(
        :class="{ 'bg-dark text-white': $darkMode }",
        v-for="batch in batches",
        :key="batch.id",
        :disabled="batch.state == 'Inactive'"
      )
        .card-body
          .row
            .col-md
              h5.card-title.mb-1.mb-md-3 {{ batch.batchName }}
              span.badge.badge-primary.mr-2 {{ batch.batchSize }} codes
              span.badge.badge-secondary {{ batch.dateActive | formatDate }} - {{ batch.dateExpires | formatDate }}
            .col-md.mt-3.mt-md-0.batch-controls
              button.btn.btn-sm.btn-primary(@click="ViewBatch(batch.id)") View Codes
              button.btn.btn-sm.btn-danger(@click="DeactivateBatch(batch)") Deactivate

      //- Alert when there are no batches
      p.text-muted.mt-3.mb-0(v-if="batches.length == 0", role="alert") No batches have been created yet.
</template>

<script>
function formatDate(date) {
  var day = ("0" + date.getDate()).slice(-2);
  var month = ("0" + (date.getMonth() + 1)).slice(-2);
  var dateString = date.getFullYear() + "-" + month + "-" + day;
  return dateString;
}

var today = new Date();
var tomorrow = new Date();
tomorrow.setDate(tomorrow.getDate() + 1);

var dateActiveMin = formatDate(today);
var dateExpiresMin = formatDate(tomorrow);

import CodeTable from "../components/CodeTable";
import { HTTP } from "../js/http-common";

module.exports = {
  name: "Admin",
  data: function () {
    return {
      batchSelected: false,
      batchIDSelected: "",
      batchName: "",
      promotionType: "",
      batchSize: 0,
      batchError: false,
      dateActive: "",
      dateExpires: "",
      batches: [],
      dateActiveMin,
      dateExpiresMin,
      formSubmitted: false,
      codeSearchVal: "",
      isSearching: false,
    };
  },
  components: {
    CodeTable,
  },
  methods: {
    CreateBatch() {
      if (this.batchSize > 0) {
        HTTP({
          method: "post",
          url: "batch",
          data: {
            batchName: this.batchName,
            batchSize: parseInt(this.batchSize),
            promotionType: this.promotionType,
            dateActive: this.dateActive,
            dateExpires: this.dateExpires,
          },
          headers: {
            "Content-Type": "application/json",
          },
        })
          .then((response) => {
            this.GetBatches();
            this.batchError = false;
          })
          .catch((e) => {
            this.batchError = true;
          });
      }
      this.formSubmitted = true;
    },
    GetBatches() {
      HTTP({
        method: "get",
        url: "batch",
      }).then((response) => {
        this.batches = response.data;
      });
    },
    ViewBatch(id) {
      this.batchSelected = true;
      this.batchIDSelected = id;
    },
    DeactivateBatch(batch) {
      HTTP({
        method: "delete",
        url: "batch",
        data: {
          codeIDStart: parseInt(batch.codeIDStart),
          codeIDEnd: parseInt(batch.codeIDEnd),
        },
      }).then(() => {});
    },
    GoBack() {
      this.batchSelected = false;
      this.isSearching = false;
    },
    SearchCodes() {
      this.isSearching = true;
    },
  },
  created() {
    this.GetBatches();
  },
  filters: {
    formatDate: function (dateString) {
      var date = new Date(dateString);
      return (
        date.getMonth() + 1 + "/" + date.getDate() + "/" + date.getFullYear()
      );
    },
  },
};
</script>

<style lang="scss" scoped>
.card {
  margin: 30px 15px;
}

.batch {
  .batch-controls {
    .btn {
      display: block;
      width: 100%;

      @media (min-width: 768px) {
        max-width: 200px;
        margin-left: auto;
      }

      &:first-child {
        margin-bottom: 10px;
      }
    }
  }
}
</style>