using System;

using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public static unsafe partial class Methods
    {
        [NativeTypeName("#define FLAC_API_VERSION_CURRENT 11")]
        public const int FLAC_API_VERSION_CURRENT = 11;

        [NativeTypeName("#define FLAC_API_VERSION_REVISION 0")]
        public const int FLAC_API_VERSION_REVISION = 0;

        [NativeTypeName("#define FLAC_API_VERSION_AGE 3")]
        public const int FLAC_API_VERSION_AGE = 3;

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__format_sample_rate_is_valid", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int format_sample_rate_is_valid([NativeTypeName("uint32_t")] uint sample_rate);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__format_blocksize_is_subset", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int format_blocksize_is_subset([NativeTypeName("uint32_t")] uint blocksize, [NativeTypeName("uint32_t")] uint sample_rate);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__format_sample_rate_is_subset", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int format_sample_rate_is_subset([NativeTypeName("uint32_t")] uint sample_rate);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__format_vorbiscomment_entry_name_is_legal", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int format_vorbiscomment_entry_name_is_legal([NativeTypeName("const char *")] sbyte* name);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__format_vorbiscomment_entry_value_is_legal", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int format_vorbiscomment_entry_value_is_legal([NativeTypeName("const FLAC__byte *")] byte* value, [NativeTypeName("uint32_t")] uint length);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__format_vorbiscomment_entry_is_legal", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int format_vorbiscomment_entry_is_legal([NativeTypeName("const FLAC__byte *")] byte* entry, [NativeTypeName("uint32_t")] uint length);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__format_seektable_is_legal", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int format_seektable_is_legal([NativeTypeName("const FLAC__StreamMetadata_SeekTable *")] FLAC__StreamMetadata_SeekTable* seek_table);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__format_seektable_sort", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint format_seektable_sort(FLAC__StreamMetadata_SeekTable* seek_table);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__format_cuesheet_is_legal", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int format_cuesheet_is_legal([NativeTypeName("const FLAC__StreamMetadata_CueSheet *")] FLAC__StreamMetadata_CueSheet* cue_sheet, [NativeTypeName("FLAC__bool")] int check_cd_da_subset, [NativeTypeName("const char **")] sbyte** violation);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__format_picture_is_legal", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int format_picture_is_legal([NativeTypeName("const FLAC__StreamMetadata_Picture *")] FLAC__StreamMetadata_Picture* picture, [NativeTypeName("const char **")] sbyte** violation);

        [NativeTypeName("#define FLAC__MAX_METADATA_TYPE_CODE (126u)")]
        public const uint FLAC__MAX_METADATA_TYPE_CODE = (126U);

        [NativeTypeName("#define FLAC__MIN_BLOCK_SIZE (16u)")]
        public const uint FLAC__MIN_BLOCK_SIZE = (16U);

        [NativeTypeName("#define FLAC__MAX_BLOCK_SIZE (65535u)")]
        public const uint FLAC__MAX_BLOCK_SIZE = (65535U);

        [NativeTypeName("#define FLAC__SUBSET_MAX_BLOCK_SIZE_48000HZ (4608u)")]
        public const uint FLAC__SUBSET_MAX_BLOCK_SIZE_48000HZ = (4608U);

        [NativeTypeName("#define FLAC__MAX_CHANNELS (8u)")]
        public const uint FLAC__MAX_CHANNELS = (8U);

        [NativeTypeName("#define FLAC__MIN_BITS_PER_SAMPLE (4u)")]
        public const uint FLAC__MIN_BITS_PER_SAMPLE = (4U);

        [NativeTypeName("#define FLAC__MAX_BITS_PER_SAMPLE (32u)")]
        public const uint FLAC__MAX_BITS_PER_SAMPLE = (32U);

        [NativeTypeName("#define FLAC__REFERENCE_CODEC_MAX_BITS_PER_SAMPLE (24u)")]
        public const uint FLAC__REFERENCE_CODEC_MAX_BITS_PER_SAMPLE = (24U);

        [NativeTypeName("#define FLAC__MAX_SAMPLE_RATE (655350u)")]
        public const uint FLAC__MAX_SAMPLE_RATE = (655350U);

        [NativeTypeName("#define FLAC__MAX_LPC_ORDER (32u)")]
        public const uint FLAC__MAX_LPC_ORDER = (32U);

        [NativeTypeName("#define FLAC__SUBSET_MAX_LPC_ORDER_48000HZ (12u)")]
        public const uint FLAC__SUBSET_MAX_LPC_ORDER_48000HZ = (12U);

        [NativeTypeName("#define FLAC__MIN_QLP_COEFF_PRECISION (5u)")]
        public const uint FLAC__MIN_QLP_COEFF_PRECISION = (5U);

        [NativeTypeName("#define FLAC__MAX_QLP_COEFF_PRECISION (15u)")]
        public const uint FLAC__MAX_QLP_COEFF_PRECISION = (15U);

        [NativeTypeName("#define FLAC__MAX_FIXED_ORDER (4u)")]
        public const uint FLAC__MAX_FIXED_ORDER = (4U);

        [NativeTypeName("#define FLAC__MAX_RICE_PARTITION_ORDER (15u)")]
        public const uint FLAC__MAX_RICE_PARTITION_ORDER = (15U);

        [NativeTypeName("#define FLAC__SUBSET_MAX_RICE_PARTITION_ORDER (8u)")]
        public const uint FLAC__SUBSET_MAX_RICE_PARTITION_ORDER = (8U);

        [NativeTypeName("#define FLAC__STREAM_SYNC_LENGTH (4u)")]
        public const uint FLAC__STREAM_SYNC_LENGTH = (4U);

        [NativeTypeName("#define FLAC__STREAM_METADATA_STREAMINFO_LENGTH (34u)")]
        public const uint FLAC__STREAM_METADATA_STREAMINFO_LENGTH = (34U);

        [NativeTypeName("#define FLAC__STREAM_METADATA_SEEKPOINT_LENGTH (18u)")]
        public const uint FLAC__STREAM_METADATA_SEEKPOINT_LENGTH = (18U);

        [NativeTypeName("#define FLAC__STREAM_METADATA_HEADER_LENGTH (4u)")]
        public const uint FLAC__STREAM_METADATA_HEADER_LENGTH = (4U);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_get_streaminfo", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_get_streaminfo([NativeTypeName("const char *")] sbyte* filename, FLAC__StreamMetadata* streaminfo);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_get_tags", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_get_tags([NativeTypeName("const char *")] sbyte* filename, FLAC__StreamMetadata** tags);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_get_cuesheet", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_get_cuesheet([NativeTypeName("const char *")] sbyte* filename, FLAC__StreamMetadata** cuesheet);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_get_picture", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_get_picture([NativeTypeName("const char *")] sbyte* filename, FLAC__StreamMetadata** picture, FLAC__StreamMetadata_Picture_Type type, [NativeTypeName("const char *")] sbyte* mime_type, [NativeTypeName("const FLAC__byte *")] byte* description, [NativeTypeName("uint32_t")] uint max_width, [NativeTypeName("uint32_t")] uint max_height, [NativeTypeName("uint32_t")] uint max_depth, [NativeTypeName("uint32_t")] uint max_colors);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_new", ExactSpelling = true)]
        public static extern FLAC__Metadata_SimpleIterator* metadata_simple_iterator_new();

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_delete", ExactSpelling = true)]
        public static extern void metadata_simple_iterator_delete(FLAC__Metadata_SimpleIterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_status", ExactSpelling = true)]
        public static extern FLAC__Metadata_SimpleIteratorStatus metadata_simple_iterator_status(FLAC__Metadata_SimpleIterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_init", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_simple_iterator_init(FLAC__Metadata_SimpleIterator* iterator, [NativeTypeName("const char *")] sbyte* filename, [NativeTypeName("FLAC__bool")] int read_only, [NativeTypeName("FLAC__bool")] int preserve_file_stats);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_is_writable", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_simple_iterator_is_writable([NativeTypeName("const FLAC__Metadata_SimpleIterator *")] FLAC__Metadata_SimpleIterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_next", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_simple_iterator_next(FLAC__Metadata_SimpleIterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_prev", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_simple_iterator_prev(FLAC__Metadata_SimpleIterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_is_last", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_simple_iterator_is_last([NativeTypeName("const FLAC__Metadata_SimpleIterator *")] FLAC__Metadata_SimpleIterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_get_block_offset", ExactSpelling = true)]
        [return: NativeTypeName("off_t")]
        public static extern int metadata_simple_iterator_get_block_offset([NativeTypeName("const FLAC__Metadata_SimpleIterator *")] FLAC__Metadata_SimpleIterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_get_block_type", ExactSpelling = true)]
        public static extern FLAC__MetadataType metadata_simple_iterator_get_block_type([NativeTypeName("const FLAC__Metadata_SimpleIterator *")] FLAC__Metadata_SimpleIterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_get_block_length", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint metadata_simple_iterator_get_block_length([NativeTypeName("const FLAC__Metadata_SimpleIterator *")] FLAC__Metadata_SimpleIterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_get_application_id", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_simple_iterator_get_application_id(FLAC__Metadata_SimpleIterator* iterator, [NativeTypeName("FLAC__byte *")] byte* id);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_get_block", ExactSpelling = true)]
        public static extern FLAC__StreamMetadata* metadata_simple_iterator_get_block(FLAC__Metadata_SimpleIterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_set_block", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_simple_iterator_set_block(FLAC__Metadata_SimpleIterator* iterator, FLAC__StreamMetadata* block, [NativeTypeName("FLAC__bool")] int use_padding);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_insert_block_after", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_simple_iterator_insert_block_after(FLAC__Metadata_SimpleIterator* iterator, FLAC__StreamMetadata* block, [NativeTypeName("FLAC__bool")] int use_padding);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_simple_iterator_delete_block", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_simple_iterator_delete_block(FLAC__Metadata_SimpleIterator* iterator, [NativeTypeName("FLAC__bool")] int use_padding);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_chain_new", ExactSpelling = true)]
        public static extern FLAC__Metadata_Chain* metadata_chain_new();

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_chain_delete", ExactSpelling = true)]
        public static extern void metadata_chain_delete(FLAC__Metadata_Chain* chain);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_chain_status", ExactSpelling = true)]
        public static extern FLAC__Metadata_ChainStatus metadata_chain_status(FLAC__Metadata_Chain* chain);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_chain_read", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_chain_read(FLAC__Metadata_Chain* chain, [NativeTypeName("const char *")] sbyte* filename);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_chain_read_ogg", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_chain_read_ogg(FLAC__Metadata_Chain* chain, [NativeTypeName("const char *")] sbyte* filename);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_chain_read_with_callbacks", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_chain_read_with_callbacks(FLAC__Metadata_Chain* chain, [NativeTypeName("FLAC__IOHandle")] void* handle, FLAC__IOCallbacks callbacks);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_chain_read_ogg_with_callbacks", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_chain_read_ogg_with_callbacks(FLAC__Metadata_Chain* chain, [NativeTypeName("FLAC__IOHandle")] void* handle, FLAC__IOCallbacks callbacks);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_chain_check_if_tempfile_needed", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_chain_check_if_tempfile_needed(FLAC__Metadata_Chain* chain, [NativeTypeName("FLAC__bool")] int use_padding);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_chain_write", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_chain_write(FLAC__Metadata_Chain* chain, [NativeTypeName("FLAC__bool")] int use_padding, [NativeTypeName("FLAC__bool")] int preserve_file_stats);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_chain_write_with_callbacks", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_chain_write_with_callbacks(FLAC__Metadata_Chain* chain, [NativeTypeName("FLAC__bool")] int use_padding, [NativeTypeName("FLAC__IOHandle")] void* handle, FLAC__IOCallbacks callbacks);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_chain_write_with_callbacks_and_tempfile", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_chain_write_with_callbacks_and_tempfile(FLAC__Metadata_Chain* chain, [NativeTypeName("FLAC__bool")] int use_padding, [NativeTypeName("FLAC__IOHandle")] void* handle, FLAC__IOCallbacks callbacks, [NativeTypeName("FLAC__IOHandle")] void* temp_handle, FLAC__IOCallbacks temp_callbacks);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_chain_merge_padding", ExactSpelling = true)]
        public static extern void metadata_chain_merge_padding(FLAC__Metadata_Chain* chain);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_chain_sort_padding", ExactSpelling = true)]
        public static extern void metadata_chain_sort_padding(FLAC__Metadata_Chain* chain);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_iterator_new", ExactSpelling = true)]
        public static extern FLAC__Metadata_Iterator* metadata_iterator_new();

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_iterator_delete", ExactSpelling = true)]
        public static extern void metadata_iterator_delete(FLAC__Metadata_Iterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_iterator_init", ExactSpelling = true)]
        public static extern void metadata_iterator_init(FLAC__Metadata_Iterator* iterator, FLAC__Metadata_Chain* chain);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_iterator_next", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_iterator_next(FLAC__Metadata_Iterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_iterator_prev", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_iterator_prev(FLAC__Metadata_Iterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_iterator_get_block_type", ExactSpelling = true)]
        public static extern FLAC__MetadataType metadata_iterator_get_block_type([NativeTypeName("const FLAC__Metadata_Iterator *")] FLAC__Metadata_Iterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_iterator_get_block", ExactSpelling = true)]
        public static extern FLAC__StreamMetadata* metadata_iterator_get_block(FLAC__Metadata_Iterator* iterator);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_iterator_set_block", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_iterator_set_block(FLAC__Metadata_Iterator* iterator, FLAC__StreamMetadata* block);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_iterator_delete_block", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_iterator_delete_block(FLAC__Metadata_Iterator* iterator, [NativeTypeName("FLAC__bool")] int replace_with_padding);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_iterator_insert_block_before", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_iterator_insert_block_before(FLAC__Metadata_Iterator* iterator, FLAC__StreamMetadata* block);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_iterator_insert_block_after", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_iterator_insert_block_after(FLAC__Metadata_Iterator* iterator, FLAC__StreamMetadata* block);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_new", ExactSpelling = true)]
        public static extern FLAC__StreamMetadata* metadata_object_new(FLAC__MetadataType type);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_clone", ExactSpelling = true)]
        public static extern FLAC__StreamMetadata* metadata_object_clone([NativeTypeName("const FLAC__StreamMetadata *")] FLAC__StreamMetadata* @object);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_delete", ExactSpelling = true)]
        public static extern void metadata_object_delete(FLAC__StreamMetadata* @object);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_is_equal", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_is_equal([NativeTypeName("const FLAC__StreamMetadata *")] FLAC__StreamMetadata* block1, [NativeTypeName("const FLAC__StreamMetadata *")] FLAC__StreamMetadata* block2);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_application_set_data", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_application_set_data(FLAC__StreamMetadata* @object, [NativeTypeName("FLAC__byte *")] byte* data, [NativeTypeName("uint32_t")] uint length, [NativeTypeName("FLAC__bool")] int copy);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_seektable_resize_points", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_seektable_resize_points(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint new_num_points);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_seektable_set_point", ExactSpelling = true)]
        public static extern void metadata_object_seektable_set_point(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint point_num, FLAC__StreamMetadata_SeekPoint point);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_seektable_insert_point", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_seektable_insert_point(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint point_num, FLAC__StreamMetadata_SeekPoint point);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_seektable_delete_point", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_seektable_delete_point(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint point_num);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_seektable_is_legal", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_seektable_is_legal([NativeTypeName("const FLAC__StreamMetadata *")] FLAC__StreamMetadata* @object);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_seektable_template_append_placeholders", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_seektable_template_append_placeholders(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint num);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_seektable_template_append_point", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_seektable_template_append_point(FLAC__StreamMetadata* @object, [NativeTypeName("FLAC__uint64")] ulong sample_number);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_seektable_template_append_points", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_seektable_template_append_points(FLAC__StreamMetadata* @object, [NativeTypeName("FLAC__uint64 []")] ulong* sample_numbers, [NativeTypeName("uint32_t")] uint num);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_seektable_template_append_spaced_points", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_seektable_template_append_spaced_points(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint num, [NativeTypeName("FLAC__uint64")] ulong total_samples);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_seektable_template_append_spaced_points_by_samples", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_seektable_template_append_spaced_points_by_samples(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint samples, [NativeTypeName("FLAC__uint64")] ulong total_samples);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_seektable_template_sort", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_seektable_template_sort(FLAC__StreamMetadata* @object, [NativeTypeName("FLAC__bool")] int compact);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_vorbiscomment_set_vendor_string", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_vorbiscomment_set_vendor_string(FLAC__StreamMetadata* @object, FLAC__StreamMetadata_VorbisComment_Entry entry, [NativeTypeName("FLAC__bool")] int copy);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_vorbiscomment_resize_comments", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_vorbiscomment_resize_comments(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint new_num_comments);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_vorbiscomment_set_comment", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_vorbiscomment_set_comment(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint comment_num, FLAC__StreamMetadata_VorbisComment_Entry entry, [NativeTypeName("FLAC__bool")] int copy);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_vorbiscomment_insert_comment", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_vorbiscomment_insert_comment(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint comment_num, FLAC__StreamMetadata_VorbisComment_Entry entry, [NativeTypeName("FLAC__bool")] int copy);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_vorbiscomment_append_comment", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_vorbiscomment_append_comment(FLAC__StreamMetadata* @object, FLAC__StreamMetadata_VorbisComment_Entry entry, [NativeTypeName("FLAC__bool")] int copy);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_vorbiscomment_replace_comment", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_vorbiscomment_replace_comment(FLAC__StreamMetadata* @object, FLAC__StreamMetadata_VorbisComment_Entry entry, [NativeTypeName("FLAC__bool")] int all, [NativeTypeName("FLAC__bool")] int copy);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_vorbiscomment_delete_comment", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_vorbiscomment_delete_comment(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint comment_num);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_vorbiscomment_entry_from_name_value_pair", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_vorbiscomment_entry_from_name_value_pair(FLAC__StreamMetadata_VorbisComment_Entry* entry, [NativeTypeName("const char *")] sbyte* field_name, [NativeTypeName("const char *")] sbyte* field_value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_vorbiscomment_entry_to_name_value_pair", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_vorbiscomment_entry_to_name_value_pair([NativeTypeName("const FLAC__StreamMetadata_VorbisComment_Entry")] FLAC__StreamMetadata_VorbisComment_Entry entry, [NativeTypeName("char **")] sbyte** field_name, [NativeTypeName("char **")] sbyte** field_value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_vorbiscomment_entry_matches", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_vorbiscomment_entry_matches([NativeTypeName("const FLAC__StreamMetadata_VorbisComment_Entry")] FLAC__StreamMetadata_VorbisComment_Entry entry, [NativeTypeName("const char *")] sbyte* field_name, [NativeTypeName("uint32_t")] uint field_name_length);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_vorbiscomment_find_entry_from", ExactSpelling = true)]
        public static extern int metadata_object_vorbiscomment_find_entry_from([NativeTypeName("const FLAC__StreamMetadata *")] FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint offset, [NativeTypeName("const char *")] sbyte* field_name);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_vorbiscomment_remove_entry_matching", ExactSpelling = true)]
        public static extern int metadata_object_vorbiscomment_remove_entry_matching(FLAC__StreamMetadata* @object, [NativeTypeName("const char *")] sbyte* field_name);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_vorbiscomment_remove_entries_matching", ExactSpelling = true)]
        public static extern int metadata_object_vorbiscomment_remove_entries_matching(FLAC__StreamMetadata* @object, [NativeTypeName("const char *")] sbyte* field_name);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_track_new", ExactSpelling = true)]
        public static extern FLAC__StreamMetadata_CueSheet_Track* metadata_object_cuesheet_track_new();

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_track_clone", ExactSpelling = true)]
        public static extern FLAC__StreamMetadata_CueSheet_Track* metadata_object_cuesheet_track_clone([NativeTypeName("const FLAC__StreamMetadata_CueSheet_Track *")] FLAC__StreamMetadata_CueSheet_Track* @object);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_track_delete", ExactSpelling = true)]
        public static extern void metadata_object_cuesheet_track_delete(FLAC__StreamMetadata_CueSheet_Track* @object);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_track_resize_indices", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_cuesheet_track_resize_indices(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint track_num, [NativeTypeName("uint32_t")] uint new_num_indices);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_track_insert_index", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_cuesheet_track_insert_index(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint track_num, [NativeTypeName("uint32_t")] uint index_num, FLAC__StreamMetadata_CueSheet_Index index);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_track_insert_blank_index", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_cuesheet_track_insert_blank_index(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint track_num, [NativeTypeName("uint32_t")] uint index_num);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_track_delete_index", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_cuesheet_track_delete_index(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint track_num, [NativeTypeName("uint32_t")] uint index_num);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_resize_tracks", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_cuesheet_resize_tracks(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint new_num_tracks);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_set_track", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_cuesheet_set_track(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint track_num, FLAC__StreamMetadata_CueSheet_Track* track, [NativeTypeName("FLAC__bool")] int copy);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_insert_track", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_cuesheet_insert_track(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint track_num, FLAC__StreamMetadata_CueSheet_Track* track, [NativeTypeName("FLAC__bool")] int copy);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_insert_blank_track", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_cuesheet_insert_blank_track(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint track_num);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_delete_track", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_cuesheet_delete_track(FLAC__StreamMetadata* @object, [NativeTypeName("uint32_t")] uint track_num);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_is_legal", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_cuesheet_is_legal([NativeTypeName("const FLAC__StreamMetadata *")] FLAC__StreamMetadata* @object, [NativeTypeName("FLAC__bool")] int check_cd_da_subset, [NativeTypeName("const char **")] sbyte** violation);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_cuesheet_calculate_cddb_id", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__uint32")]
        public static extern uint metadata_object_cuesheet_calculate_cddb_id([NativeTypeName("const FLAC__StreamMetadata *")] FLAC__StreamMetadata* @object);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_picture_set_mime_type", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_picture_set_mime_type(FLAC__StreamMetadata* @object, [NativeTypeName("char *")] sbyte* mime_type, [NativeTypeName("FLAC__bool")] int copy);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_picture_set_description", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_picture_set_description(FLAC__StreamMetadata* @object, [NativeTypeName("FLAC__byte *")] byte* description, [NativeTypeName("FLAC__bool")] int copy);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_picture_set_data", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_picture_set_data(FLAC__StreamMetadata* @object, [NativeTypeName("FLAC__byte *")] byte* data, [NativeTypeName("FLAC__uint32")] uint length, [NativeTypeName("FLAC__bool")] int copy);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__metadata_object_picture_is_legal", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int metadata_object_picture_is_legal([NativeTypeName("const FLAC__StreamMetadata *")] FLAC__StreamMetadata* @object, [NativeTypeName("const char **")] sbyte** violation);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_new", ExactSpelling = true)]
        public static extern FLAC__StreamDecoder* stream_decoder_new();

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_delete", ExactSpelling = true)]
        public static extern void stream_decoder_delete(FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_set_ogg_serial_number", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_set_ogg_serial_number(FLAC__StreamDecoder* decoder, [NativeTypeName("long")] int serial_number);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_set_md5_checking", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_set_md5_checking(FLAC__StreamDecoder* decoder, [NativeTypeName("FLAC__bool")] int value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_set_metadata_respond", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_set_metadata_respond(FLAC__StreamDecoder* decoder, FLAC__MetadataType type);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_set_metadata_respond_application", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_set_metadata_respond_application(FLAC__StreamDecoder* decoder, [NativeTypeName("const FLAC__byte [4]")] byte* id);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_set_metadata_respond_all", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_set_metadata_respond_all(FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_set_metadata_ignore", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_set_metadata_ignore(FLAC__StreamDecoder* decoder, FLAC__MetadataType type);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_set_metadata_ignore_application", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_set_metadata_ignore_application(FLAC__StreamDecoder* decoder, [NativeTypeName("const FLAC__byte [4]")] byte* id);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_set_metadata_ignore_all", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_set_metadata_ignore_all(FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_get_state", ExactSpelling = true)]
        public static extern FLAC__StreamDecoderState stream_decoder_get_state([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_get_resolved_state_string", ExactSpelling = true)]
        [return: NativeTypeName("const char *")]
        public static extern sbyte* stream_decoder_get_resolved_state_string([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_get_md5_checking", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_get_md5_checking([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_get_total_samples", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__uint64")]
        public static extern ulong stream_decoder_get_total_samples([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_get_channels", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint stream_decoder_get_channels([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_get_channel_assignment", ExactSpelling = true)]
        public static extern FLAC__ChannelAssignment stream_decoder_get_channel_assignment([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_get_bits_per_sample", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint stream_decoder_get_bits_per_sample([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_get_sample_rate", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint stream_decoder_get_sample_rate([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_get_blocksize", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint stream_decoder_get_blocksize([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_get_decode_position", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_get_decode_position([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder, [NativeTypeName("FLAC__uint64 *")] ulong* position);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_init_stream", ExactSpelling = true)]
        public static extern FLAC__StreamDecoderInitStatus stream_decoder_init_stream(FLAC__StreamDecoder* decoder, [NativeTypeName("FLAC__StreamDecoderReadCallback")] IntPtr read_callback, [NativeTypeName("FLAC__StreamDecoderSeekCallback")] IntPtr seek_callback, [NativeTypeName("FLAC__StreamDecoderTellCallback")] IntPtr tell_callback, [NativeTypeName("FLAC__StreamDecoderLengthCallback")] IntPtr length_callback, [NativeTypeName("FLAC__StreamDecoderEofCallback")] IntPtr eof_callback, [NativeTypeName("FLAC__StreamDecoderWriteCallback")] IntPtr write_callback, [NativeTypeName("FLAC__StreamDecoderMetadataCallback")] IntPtr metadata_callback, [NativeTypeName("FLAC__StreamDecoderErrorCallback")] IntPtr error_callback, void* client_data);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_init_ogg_stream", ExactSpelling = true)]
        public static extern FLAC__StreamDecoderInitStatus stream_decoder_init_ogg_stream(FLAC__StreamDecoder* decoder, [NativeTypeName("FLAC__StreamDecoderReadCallback")] IntPtr read_callback, [NativeTypeName("FLAC__StreamDecoderSeekCallback")] IntPtr seek_callback, [NativeTypeName("FLAC__StreamDecoderTellCallback")] IntPtr tell_callback, [NativeTypeName("FLAC__StreamDecoderLengthCallback")] IntPtr length_callback, [NativeTypeName("FLAC__StreamDecoderEofCallback")] IntPtr eof_callback, [NativeTypeName("FLAC__StreamDecoderWriteCallback")] IntPtr write_callback, [NativeTypeName("FLAC__StreamDecoderMetadataCallback")] IntPtr metadata_callback, [NativeTypeName("FLAC__StreamDecoderErrorCallback")] IntPtr error_callback, void* client_data);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_init_FILE", ExactSpelling = true)]
        public static extern FLAC__StreamDecoderInitStatus stream_decoder_init_FILE(FLAC__StreamDecoder* decoder, [NativeTypeName("FILE *")] _iobuf* file, [NativeTypeName("FLAC__StreamDecoderWriteCallback")] IntPtr write_callback, [NativeTypeName("FLAC__StreamDecoderMetadataCallback")] IntPtr metadata_callback, [NativeTypeName("FLAC__StreamDecoderErrorCallback")] IntPtr error_callback, void* client_data);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_init_ogg_FILE", ExactSpelling = true)]
        public static extern FLAC__StreamDecoderInitStatus stream_decoder_init_ogg_FILE(FLAC__StreamDecoder* decoder, [NativeTypeName("FILE *")] _iobuf* file, [NativeTypeName("FLAC__StreamDecoderWriteCallback")] IntPtr write_callback, [NativeTypeName("FLAC__StreamDecoderMetadataCallback")] IntPtr metadata_callback, [NativeTypeName("FLAC__StreamDecoderErrorCallback")] IntPtr error_callback, void* client_data);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_init_file", ExactSpelling = true)]
        public static extern FLAC__StreamDecoderInitStatus stream_decoder_init_file(FLAC__StreamDecoder* decoder, [NativeTypeName("const char *")] sbyte* filename, [NativeTypeName("FLAC__StreamDecoderWriteCallback")] IntPtr write_callback, [NativeTypeName("FLAC__StreamDecoderMetadataCallback")] IntPtr metadata_callback, [NativeTypeName("FLAC__StreamDecoderErrorCallback")] IntPtr error_callback, void* client_data);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_init_ogg_file", ExactSpelling = true)]
        public static extern FLAC__StreamDecoderInitStatus stream_decoder_init_ogg_file(FLAC__StreamDecoder* decoder, [NativeTypeName("const char *")] sbyte* filename, [NativeTypeName("FLAC__StreamDecoderWriteCallback")] IntPtr write_callback, [NativeTypeName("FLAC__StreamDecoderMetadataCallback")] IntPtr metadata_callback, [NativeTypeName("FLAC__StreamDecoderErrorCallback")] IntPtr error_callback, void* client_data);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_finish", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_finish(FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_flush", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_flush(FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_reset", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_reset(FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_process_single", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_process_single(FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_process_until_end_of_metadata", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_process_until_end_of_metadata(FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_process_until_end_of_stream", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_process_until_end_of_stream(FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_skip_single_frame", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_skip_single_frame(FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_decoder_seek_absolute", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_decoder_seek_absolute(FLAC__StreamDecoder* decoder, [NativeTypeName("FLAC__uint64")] ulong sample);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__get_decoder_client_data", ExactSpelling = true)]
        [return: NativeTypeName("const void *")]
        public static extern void* get_decoder_client_data(FLAC__StreamDecoder* decoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_new", ExactSpelling = true)]
        public static extern FLAC__StreamEncoder* stream_encoder_new();

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_delete", ExactSpelling = true)]
        public static extern void stream_encoder_delete(FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_ogg_serial_number", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_ogg_serial_number(FLAC__StreamEncoder* encoder, [NativeTypeName("long")] int serial_number);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_verify", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_verify(FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__bool")] int value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_streamable_subset", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_streamable_subset(FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__bool")] int value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_channels", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_channels(FLAC__StreamEncoder* encoder, [NativeTypeName("uint32_t")] uint value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_bits_per_sample", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_bits_per_sample(FLAC__StreamEncoder* encoder, [NativeTypeName("uint32_t")] uint value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_sample_rate", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_sample_rate(FLAC__StreamEncoder* encoder, [NativeTypeName("uint32_t")] uint value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_compression_level", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_compression_level(FLAC__StreamEncoder* encoder, [NativeTypeName("uint32_t")] uint value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_blocksize", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_blocksize(FLAC__StreamEncoder* encoder, [NativeTypeName("uint32_t")] uint value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_do_mid_side_stereo", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_do_mid_side_stereo(FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__bool")] int value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_loose_mid_side_stereo", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_loose_mid_side_stereo(FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__bool")] int value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_apodization", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_apodization(FLAC__StreamEncoder* encoder, [NativeTypeName("const char *")] sbyte* specification);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_max_lpc_order", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_max_lpc_order(FLAC__StreamEncoder* encoder, [NativeTypeName("uint32_t")] uint value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_qlp_coeff_precision", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_qlp_coeff_precision(FLAC__StreamEncoder* encoder, [NativeTypeName("uint32_t")] uint value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_do_qlp_coeff_prec_search", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_do_qlp_coeff_prec_search(FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__bool")] int value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_do_escape_coding", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_do_escape_coding(FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__bool")] int value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_do_exhaustive_model_search", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_do_exhaustive_model_search(FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__bool")] int value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_min_residual_partition_order", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_min_residual_partition_order(FLAC__StreamEncoder* encoder, [NativeTypeName("uint32_t")] uint value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_max_residual_partition_order", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_max_residual_partition_order(FLAC__StreamEncoder* encoder, [NativeTypeName("uint32_t")] uint value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_rice_parameter_search_dist", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_rice_parameter_search_dist(FLAC__StreamEncoder* encoder, [NativeTypeName("uint32_t")] uint value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_total_samples_estimate", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_total_samples_estimate(FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__uint64")] ulong value);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_set_metadata", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_set_metadata(FLAC__StreamEncoder* encoder, FLAC__StreamMetadata** metadata, [NativeTypeName("uint32_t")] uint num_blocks);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_state", ExactSpelling = true)]
        public static extern FLAC__StreamEncoderState stream_encoder_get_state([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_verify_decoder_state", ExactSpelling = true)]
        public static extern FLAC__StreamDecoderState stream_encoder_get_verify_decoder_state([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_resolved_state_string", ExactSpelling = true)]
        [return: NativeTypeName("const char *")]
        public static extern sbyte* stream_encoder_get_resolved_state_string([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_verify_decoder_error_stats", ExactSpelling = true)]
        public static extern void stream_encoder_get_verify_decoder_error_stats([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__uint64 *")] ulong* absolute_sample, [NativeTypeName("uint32_t *")] uint* frame_number, [NativeTypeName("uint32_t *")] uint* channel, [NativeTypeName("uint32_t *")] uint* sample, [NativeTypeName("FLAC__int32 *")] int* expected, [NativeTypeName("FLAC__int32 *")] int* got);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_verify", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_get_verify([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_streamable_subset", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_get_streamable_subset([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_channels", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint stream_encoder_get_channels([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_bits_per_sample", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint stream_encoder_get_bits_per_sample([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_sample_rate", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint stream_encoder_get_sample_rate([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_blocksize", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint stream_encoder_get_blocksize([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_do_mid_side_stereo", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_get_do_mid_side_stereo([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_loose_mid_side_stereo", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_get_loose_mid_side_stereo([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_max_lpc_order", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint stream_encoder_get_max_lpc_order([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_qlp_coeff_precision", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint stream_encoder_get_qlp_coeff_precision([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_do_qlp_coeff_prec_search", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_get_do_qlp_coeff_prec_search([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_do_escape_coding", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_get_do_escape_coding([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_do_exhaustive_model_search", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_get_do_exhaustive_model_search([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_min_residual_partition_order", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint stream_encoder_get_min_residual_partition_order([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_max_residual_partition_order", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint stream_encoder_get_max_residual_partition_order([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_rice_parameter_search_dist", ExactSpelling = true)]
        [return: NativeTypeName("uint32_t")]
        public static extern uint stream_encoder_get_rice_parameter_search_dist([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_get_total_samples_estimate", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__uint64")]
        public static extern ulong stream_encoder_get_total_samples_estimate([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_init_stream", ExactSpelling = true)]
        public static extern FLAC__StreamEncoderInitStatus stream_encoder_init_stream(FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__StreamEncoderWriteCallback")] IntPtr write_callback, [NativeTypeName("FLAC__StreamEncoderSeekCallback")] IntPtr seek_callback, [NativeTypeName("FLAC__StreamEncoderTellCallback")] IntPtr tell_callback, [NativeTypeName("FLAC__StreamEncoderMetadataCallback")] IntPtr metadata_callback, void* client_data);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_init_ogg_stream", ExactSpelling = true)]
        public static extern FLAC__StreamEncoderInitStatus stream_encoder_init_ogg_stream(FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__StreamEncoderReadCallback")] IntPtr read_callback, [NativeTypeName("FLAC__StreamEncoderWriteCallback")] IntPtr write_callback, [NativeTypeName("FLAC__StreamEncoderSeekCallback")] IntPtr seek_callback, [NativeTypeName("FLAC__StreamEncoderTellCallback")] IntPtr tell_callback, [NativeTypeName("FLAC__StreamEncoderMetadataCallback")] IntPtr metadata_callback, void* client_data);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_init_FILE", ExactSpelling = true)]
        public static extern FLAC__StreamEncoderInitStatus stream_encoder_init_FILE(FLAC__StreamEncoder* encoder, [NativeTypeName("FILE *")] _iobuf* file, [NativeTypeName("FLAC__StreamEncoderProgressCallback")] IntPtr progress_callback, void* client_data);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_init_ogg_FILE", ExactSpelling = true)]
        public static extern FLAC__StreamEncoderInitStatus stream_encoder_init_ogg_FILE(FLAC__StreamEncoder* encoder, [NativeTypeName("FILE *")] _iobuf* file, [NativeTypeName("FLAC__StreamEncoderProgressCallback")] IntPtr progress_callback, void* client_data);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_init_file", ExactSpelling = true)]
        public static extern FLAC__StreamEncoderInitStatus stream_encoder_init_file(FLAC__StreamEncoder* encoder, [NativeTypeName("const char *")] sbyte* filename, [NativeTypeName("FLAC__StreamEncoderProgressCallback")] IntPtr progress_callback, void* client_data);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_init_ogg_file", ExactSpelling = true)]
        public static extern FLAC__StreamEncoderInitStatus stream_encoder_init_ogg_file(FLAC__StreamEncoder* encoder, [NativeTypeName("const char *")] sbyte* filename, [NativeTypeName("FLAC__StreamEncoderProgressCallback")] IntPtr progress_callback, void* client_data);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_finish", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_finish(FLAC__StreamEncoder* encoder);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_process", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_process(FLAC__StreamEncoder* encoder, [NativeTypeName("const FLAC__int32 *const []")] int** buffer, [NativeTypeName("uint32_t")] uint samples);

        [DllImport("libFLAC", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FLAC__stream_encoder_process_interleaved", ExactSpelling = true)]
        [return: NativeTypeName("FLAC__bool")]
        public static extern int stream_encoder_process_interleaved(FLAC__StreamEncoder* encoder, [NativeTypeName("const FLAC__int32 []")] int* buffer, [NativeTypeName("uint32_t")] uint samples);
    }
}
